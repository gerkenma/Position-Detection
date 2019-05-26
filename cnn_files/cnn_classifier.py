from __future__ import absolute_import, division, print_function

import tensorflow as tf
from CreateDataset import CreateDataset
import sys
import numpy
import random
import os
import time

BATCH_SIZE = 64
MODE = "TRAIN"
OUTPUT_NODES = 11
DATA_PATH = "/share/projects/attitude_data/mb/mb_data"
TRIALS = 1
tf.logging.set_verbosity(tf.logging.INFO)

"""List of all tunable hyperparameters"""
all_params = {
    # Convolutional Layer #1 Params
    "conv1_filters": range(32, 128, 4),
    "conv1_kernel": [2, 3, 4, 5, 6],
    "activation": [tf.nn.leaky_relu, tf.nn.relu],
    "kernel_initializer": [tf.initializers.ones, tf.initializers.random_uniform, tf.initializers.zeros],

    # Convolutional Layer #2 Params
    "conv2_filters": range(32, 128, 4),
    "conv2_kernel": [2, 3, 4, 5, 6],

    # Final params
    "dropout": numpy.arange(0.2, 0.8, 0.1),
    "units": [128, 256, 512, 1024, 2048],
    "learning_rate": numpy.arange(0.001, 0.023, 0.003)
}

"""Parameters to be used in each trial of testing"""
selected_params = {}


def cnn_model_fn(features, labels, mode):
    sess = tf.Session()
    sess.run(tf.global_variables_initializer())
    """Model function for CNN"""
    input_layer = tf.reshape(features, [-1, 112, 112, 3])

    # Convolutional Layer #1
    conv1 = tf.layers.conv2d(
        inputs=input_layer,
        filters=selected_params["conv1_filters"],
        kernel_size=selected_params["conv1_kernel"],
        padding="same",
        activation=selected_params["activation"]
    )

    # Pooling Layer #1
    pool1 = tf.layers.max_pooling2d(inputs=conv1, pool_size=[2, 2], strides=2)

    # Convolutional Layer #1
    conv2 = tf.layers.conv2d(
        inputs=pool1,
        # Unable to change filter size of 2nd convolutional layer because changes output size
        # Caused mismatch with provided labels for evaluation
        # filters=selected_params["conv2_filters"],
        filters=64,
        kernel_size=selected_params["conv2_kernel"],
        padding="same",
        activation=selected_params["activation"]
    )

    # Pooling Layer #2
    pool2 = tf.layers.max_pooling2d(inputs=conv2, pool_size=[2, 2], strides=2)

    # Dense Layer
    pool2_flat = tf.reshape(pool2, [-1, int(int(input_layer.shape[1])/4)**2 * BATCH_SIZE])
    dense = tf.layers.dense(inputs=pool2_flat, units=selected_params["units"], activation=selected_params["activation"])
    dropout = tf.layers.dropout(inputs=dense, rate=selected_params["dropout"], training=(mode ==
                                                                                         tf.estimator.ModeKeys.TRAIN))

    # Logits Layer
    logits = tf.layers.dense(inputs=dropout, units=OUTPUT_NODES)

    predictions = {
        # Generate predictions (for PREDICT and EVAL mode)
        "classes": tf.argmax(input=logits, axis=1),
        # Add 'softmax_tensor' to the graph. It is used for PREDICT and by the 'logging_hook'.
        "probabilities": tf.nn.softmax(logits, name="softmax_tensor")
    }

    if mode == tf.estimator.ModeKeys.PREDICT:
        return tf.estimator.EstimatorSpec(mode=mode, predictions=predictions)

    # Calculate Loss (for both TRAIN and EVAL modes)
    loss = tf.losses.sparse_softmax_cross_entropy(labels=labels, logits=logits)

    # Configure the Training Op (for TRAIN mode)
    if mode == tf.estimator.ModeKeys.TRAIN:
        optimizer = tf.train.GradientDescentOptimizer(learning_rate=selected_params["learning_rate"])
        train_op = optimizer.minimize(
            loss=loss,
            global_step=tf.train.get_global_step()
        )
        return tf.estimator.EstimatorSpec(mode=mode, loss=loss, train_op=train_op)

    # Add evaluation metrics (for EVAL mode)
    eval_metric_ops = {
        "accuracy": tf.metrics.accuracy(
            labels=labels, predictions=predictions["classes"])
    }
    return tf.estimator.EstimatorSpec(
        mode=mode, loss=loss, eval_metric_ops=eval_metric_ops
    )


def main(unused_argv):
    dataset_creator = CreateDataset(DATA_PATH, MODE)
    os.system('rm -rf /share/projects/attitude/cnn_files/tmp/space_classifier_model_*')

    # Set up logging for predictions
    tensors_to_log = {"probabilities": "softmax_tensor"}
    logging_hook = tf.train.LoggingTensorHook(tensors=tensors_to_log, every_n_iter=50)

    if MODE == "TRAIN":
        # Load training and eval data
        train_ds, eval_ds = dataset_creator.create_dataset()

        def train_input_fn(dataset):
            iterator = dataset.make_one_shot_iterator()
            features, labels = iterator.get_next()
            return features, labels

        def eval_input_fn(dataset):
            iterator = dataset.make_one_shot_iterator()
            features, labels = iterator.get_next()
            return features, labels

        with open('cnn_results.csv', 'w+') as file:
            for i in range(TRIALS):
                # Model data logged to ./tmp/space_classifier_model_{VARIABLE}
                # New directory placed
                classifier = tf.estimator.Estimator(
                    model_fn=cnn_model_fn, model_dir="/share/projects/attitude/cnn_files/tmp/space_classifier_model_"
                                                     + '%03d' % i)
                start = time.time()
                for hp in all_params:
                    selected_params[hp] = random.choice(all_params[hp])

                # Must delete Tensor Flow checkpoints in order to account for adapting CNN
                classifier.train(
                    input_fn=lambda: train_input_fn(train_ds),
                    # input_fn=train_input_fn,
                    steps=None,
                    hooks=[logging_hook])

                eval_results = classifier.evaluate(input_fn=lambda: eval_input_fn(eval_ds))
                print(eval_results)

                end = time.time()

                file.write('Trial %03d\n' % i)
                for param in selected_params:
                    file.write("%s, %s\n" % (param, selected_params[param]))
                file.write("--------\n")
                for result in eval_results:
                    file.write("%s, %s\n" % (result, eval_results[result]))
                file.write("run_time, %s\n" % (end - start))
                file.write("\n")
                file.flush()

    elif MODE == "TEST":
        pass

    elif MODE == "PREDICT":
        # Load image(s) for prediction
        predict_ds = dataset_creator.create_dataset()

        def predict_input_fn(dataset):
            iterator = dataset.make_one_shot_iterator()
            features = iterator.get_next()
            return features

        # Select appropriate trial number to run prediction on that model
        trial_number = "000"
        classifier = tf.estimator.Estimator(
            model_fn=cnn_model_fn,
            model_dir="/share/projects/attitude/cnn_files/tmp/space_classifier_model_" + trial_number)

        predict_results = classifier.predict(input_fn=lambda: predict_input_fn(predict_ds))
        results = next(predict_results, "GO")
        while results != "GO":
            print(results)
            results = next(predict_results, "GO")

    else:
        sys.stderr.write("ERR:Invalid mode passed to main function of cnn_classifier")
        exit(-1)


if __name__ == "__main__":
    tf.app.run()

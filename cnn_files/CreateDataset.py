from __future__ import absolute_import, division, print_function
import tensorflow as tf
import pathlib
import random
import sys


class CreateDataset:
    def __init__(self, data_root, mode="TRAIN", batch_size=64, sequenced=False):
        self.data_root = data_root
        self.batch_size = batch_size
        self.mode = mode
        self.sequence=sequenced

    @staticmethod
    def preprocess_image(image):
        image = tf.image.decode_png(image, channels=3)
        image = tf.image.resize_images(image, [112, 112], method=tf.image.ResizeMethod.BICUBIC)
        image /= 255.0

        return image

    def load_and_preprocess_image(self, path):
        return self.preprocess_image(tf.read_file(path))

    def process_dataset(self, dataset, size, repeat_count=1):
        if self.mode == "TRAIN":
            # If dataset too large, use smaller shuffle buffer size
            # Shuffle buffer size equal to size of dataset guarantees uniform shuffle
            # if not self.sequence: dataset = dataset.shuffle(buffer_size=size)
            if not self.sequence: dataset = dataset.shuffle(buffer_size=1000)
            dataset = dataset.repeat(repeat_count)
            dataset = dataset.batch(self.batch_size)
            return dataset

    def create_dataset(self):
        data_root = pathlib.Path(self.data_root)
        all_image_paths = list(data_root.glob('*/*'))
        all_image_paths = [str(path) for path in all_image_paths]
        if self.mode == "TRAIN" and not self.sequence: # Shuffle data only if training and not in a sequence
            random.shuffle(all_image_paths)

        example_count = len(all_image_paths)

        label_names = sorted(item.name for item in data_root.glob('*/') if item.is_dir())
        label_to_index = dict((name, index) for index,name in enumerate(label_names))
        all_image_labels = [label_to_index[pathlib.Path(path).parent.name] for path in all_image_paths]

        # To prevent overfitting, need to finish implementation of creating validation dataset
        # TODO: Find how to save dataset to use for latter prediction when selecting best trial
        """
        Training: 80% # Will be 70%
        Validation: 20% # Not yet implemented
        Testing: 20% # Will be 10%
        """
        if self.mode == "TRAIN":
            training_start = 0
            training_end = int(example_count * 0.8)
            # validation_start = training_end
            # validation_end = int(example_count * 0.9)
            testing_start = training_end
            testing_end = example_count

            # Creates dataset of image paths
            train_path_ds = tf.data.Dataset.from_tensor_slices(all_image_paths[training_start:training_end])
            # validate_path_ds = tf.data.Dataset.from_tensor_slices(all_image_labels[validation_start:validation_end])
            test_path_ds = tf.data.Dataset.from_tensor_slices(all_image_paths[testing_start:testing_end])

            # Maps each dataset value to the load_and_preprocess_image function
            # Believe this is the cause of being unable to pickle the dataset
            train_image_ds = train_path_ds.map(self.load_and_preprocess_image)
            # validate_image_ds = validate_path_ds.map(self.load_and_preprocess_image)
            test_image_ds = test_path_ds.map(self.load_and_preprocess_image)

            # Create datasets of corresponding labels for images
            train_label_ds = tf.data.Dataset.from_tensor_slices(all_image_labels[training_start:training_end])
            # validate_label_ds = tf.data.Dataset.from_tensor_slices(all_image_labels[validation_start:validation_end])
            test_label_ds = tf.data.Dataset.from_tensor_slices(all_image_labels[testing_start:testing_end])

            # Creates full datasets of image-label pairs
            train_ds = self.process_dataset(tf.data.Dataset.zip((train_image_ds, train_label_ds)), example_count)
            # validate_ds = self.process_dataset(tf.data.Dataset.zip((validate_image_ds, validate_label_ds)),
            #                                    example_count)
            test_ds = self.process_dataset(tf.data.Dataset.zip((test_image_ds, test_label_ds)), example_count)

            return train_ds, test_ds

        elif self.mode == "PREDICT":
            # Creates dataset of image paths
            predict_path_ds = tf.data.Dataset.from_tensor_slices(all_image_paths)

            # Maps each dataset value to the load_and_preprocess_image function
            # Believe this is the cause of being unable to pickle the dataset
            predict_ds = predict_path_ds.map(self.load_and_preprocess_image)

            return predict_ds

        else:
            sys.stderr.write("ERR:invalid mode passed to CreateDataset")
            exit(-1)


if __name__ == "__main__":
    tf.enable_eager_execution()
    test = CreateDataset("/share/projects/attitude/els_data/")
    trn_ds, tst_ds = test.create_dataset()
    iterator = trn_ds.make_one_shot_iterator()
    tensor = iterator.get_next()
    print(tensor)

# CubeSat Location Identification

## Team

**Developer**
> Mark Gerken
> Rohith Perla
> Jackson Wark


**Professor:**
> David Ferry, PhD.

**Faculty Mentor**
> Erin Chambers, PhD.

**Client**
> Keith Bennett

## Requirements
- Python 2.6+
- TensorFlow 1.11.x
- Numpy 1.15.x
- ARES 1.3

## Set up

1. ~ARES installed to Raspberry Pi Zero via [install guide](https://github.com/gerkenma/Position-Detection).~
2. Install [Python](https://www.python.org/downloads/)
3. If not installed, install Virtualenv with `pip install virtualenv`
4. Navigate in a terminal window to the root directory of the project and set up a virtual environment with `virtualenv venv`
5. Activate the virtual environment with `venv\Scripts\activate` on a Windows machine or `source venv\bin\activate`
	- *More complete directions can be found [here](https://virtualenv.pypa.io/en/stable/userguide/#activate-script)*
6. Install the required Python packages with `pip install -r requirements.txt

*Note: ARES software not included within repo due to private nature*
*Note: Training datasets not included within repo due to large size, however samples are available in * 

## Training

Training data generated from Unity simulation. The program [cnn_classifier.py](https://github.com/gerkenma/Position-Detection/blob/master/cnn_files/cnn_classifier.py) should be trained on external hardware, not the Raspberry Pi. Datasets may be created by pointing appropriate datapath to a directory containing subdirectories of all possible classifications. All images inside each subdirectory is assumed to belong to that classification label. All training automatically completes with an evalation of a percentage of the data and outputs the results to the file [cnn_results.csv](https://github.com/gerkenma/Position-Detection/blob/master/cnn_files/cnn_results.csv).

## Porting Trained Neural Network to Raspberry Pi Zero

Once the network is trained checkpoint files (.ckpt) will have automatically been generated in the directory specified as the model directory in cnn_classifier.py. This directory in it's entirety may be copied to the Raspberry Pi, but it may be more desirable to copy only the neccessary files due to their size. The neccessary files are those marked as 'model.ckpt-X.Y' where X is an integer and Y is either data, index, or meta. The checkpoint with the highest X is the most recent and will be used for inference. Copy all three types (or the entire directory) to the directory 'ares/aps/tmp/space_classifier_model/' after first removing its previous contents. Open the file APSReasonerLOCORI.py. Check the global dictionary 'selected_params' and replace the values of the hyperparameters with the corresponding values from cnn_results.csv for the selected trial. If changes have been made to the 'cnn_model_fn()' in cnn_classifier.py before training, replace the 'cnn_model_fn()' in APSReasonerLOCORI.py with the identical new model function from the training script. This may also warrant changes to the 'predict_input_fn()' if the input size has changed. To run inference, place one or more images in the directory 'ares/aps/tmp/predictions/predictions/'. Navigate to 'ares/aps/' and run the command 'python APSUI.py'. This will run the user interface for the APS subsystem with LOCORI loaded. The user interface will prompt several times. Type 'ta' to run the test platform, and press the the return key for the next four to run default settings. When prompted how many steps to run, press return to execute the default one step. 

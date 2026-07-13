# Training the model

Install Python

```py install 3.10```

Enter the right directory for the Unity project:

```cd 2.4-UNITY-GROEP```

Install Python 3.10 and create a virtual environment:

```py -3.10 -m venv .venv```

```.venv\Scripts\activate```

```python -m pip install --upgrade pip```

```>pip install torch==2.2.1 --index-url https://download.pytorch.org/whl/cu121```

Install the ML-Agents package and check if it is installed correctly:

```pip install mlagents==1.1.0```

Install an older version of setuptools to avoid compatibility issues with mlagents:

```pip install "setuptools<82"```

Check the installation of mlagents and its dependencies:

```mlagents-learn --help```

Run the training process (use a unique run ID for each training session):

```mlagents-learn config.yaml --run-id=sorting_run_id```

In unity clear the current model from the bot 'Clanker' in the Hiearchy tab, then in properties the model field in the behavior paramaters script should be empty. Then click on the play button to start the training process. The training will run until you stop it or until it reaches the maximum number of steps defined in the config.yaml file.

the results will be stored in results/sorting_run_id.


"""
API backend d'envoie des datas journalières, à la page web
"""

import os
from flask import Flask, render_template, request, redirect, url_for, jsonify, send_file, send_from_directory, safe_join, abort
from flask_cors import CORS
import json
import datetime

app = Flask(__name__)
CORS(app)

jsonPath = "Supervision/backend py/test.json" 
dataPath = "Sauvegarde-DATA/bin/Debug/log_data/"
       
@app.route("/getCSV", methods=["GET"])
def sendData():
    try:
        date = f"{datetime.datetime.now():%d_%m_%Y}"
        path = dataPath + date + ".csv"
        data = open(path, "r").read()
        response = app.response_class(
            response=data,
            status=200,
            mimetype='application/json'
        )
        return response
    except FileNotFoundError:
        abort(404) 

if __name__ == "__main__":
    print("API backend d'envoie des datas jounalières à la age web")
    print("Utilise les fichiers .json du jour")
    app.run(host="0.0.0.0", port=5000)

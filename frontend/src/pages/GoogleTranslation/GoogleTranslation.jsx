import React, { useState, useContext } from 'react';
import { UserContext } from "../../index";

const url = process.env.REACT_APP_MY_URL;

const translateText = async (text, targetLanguage, sourceLanguage, token) => {

  return fetch(`${url}/Translation/Translate`, {
      method: "POST",
      headers: {
      "Content-Type": "application/json",
      "Authorization": "Bearer " + token
      },
      body: JSON.stringify({
        "text": text,
        "targetLanguage": targetLanguage,
        "sourceLanguage": sourceLanguage
      }),
  }).then((res) => {
    console.log(res)
      if (!res.ok) {
          return res.json().then((data) => {
              let errorMessage = "Translation failed";
              throw new Error(errorMessage);
          });
      }
      return res.json(); //if the response is "ok"
  });
};

function GoogleTranslation() {
  const [inputText, setInputText] = useState('');
  const [targetLanguage, setTargetLanguage] = useState('hu'); //default: Hungarian
  const [sourceLanguage, setSourceLanguage] = useState('en'); //default: English
  const [translatedText, setTranslatedText] = useState('');
  const context = useContext(UserContext); //connect to UserContext - email, userName, token

  const handleTranslate = async () => {
    if (inputText) {
      const translation = await translateText(inputText, targetLanguage, sourceLanguage, context.user.token);
      console.log(translation.translatedText);
      setTranslatedText(translation.translatedText);
    }
  };

  return (
    <div className='flex items-center justify-center h-screen'>
    <div className='max-w-sm p-6 bg-slate-300 rounded-lg shadow-lg'>
      <textarea
        className="w-full h-32 p-2 mb-4 bg-white border rounded focus:outline-none focus:shadow-outline"
        type="text"
        value={inputText}
        onChange={(e) => setInputText(e.target.value)}
      />
      <select
        className="w-full p-2 mb-4 bg-white border rounded focus:outline-none focus:shadow-outline"
        value={targetLanguage}
        onChange={(e) => setTargetLanguage(e.target.value)}
      >
        <option value="es">Spanish</option>
        <option value="fr">French</option>
        <option value="es">German</option>
        <option value="fr">Italian</option>
        <option value="hu">Hungarian</option>
        {/* Add more language options */}
      </select>
      <button className="w-full bg-rose-600 hover:bg-rose-400 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline" onClick={handleTranslate}>Translate</button>
      <div className="text-center mt-4">{translatedText}</div>
    </div>
    </div>
  );
}

export default GoogleTranslation;
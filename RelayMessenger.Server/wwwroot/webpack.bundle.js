/*
 * ATTENTION: The "eval" devtool has been used (maybe by default in mode: "development").
 * This devtool is neither made for production nor for readable output files.
 * It uses "eval()" calls to create a separate source file in the browser devtools.
 * If you are trying to read the output file, select a different devtool (https://webpack.js.org/configuration/devtool/)
 * or disable the default devtool with "devtool: false".
 * If you are looking for production-ready output files, see mode: "production" (https://webpack.js.org/configuration/mode/).
 */
/******/ (() => { // webpackBootstrap
/******/ 	var __webpack_modules__ = ({

/***/ "./src/blazorDom.js":
/*!**************************!*\
  !*** ./src/blazorDom.js ***!
  \**************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony export */ __webpack_require__.d(__webpack_exports__, {\n/* harmony export */   \"getElementById\": () => (/* binding */ getElementById)\n/* harmony export */ });\nfunction getElementById(id) {\n    return document.getElementById(id);\n}\n\n//# sourceURL=webpack://npm/./src/blazorDom.js?");

/***/ }),

/***/ "./src/messageNotifications.js":
/*!*************************************!*\
  !*** ./src/messageNotifications.js ***!
  \*************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony export */ __webpack_require__.d(__webpack_exports__, {\n/* harmony export */   \"requestPermission\": () => (/* binding */ requestPermission)\n/* harmony export */ });\nasync function requestPermission() {\n    // Ask the user for permission to send notifications\n    if ('Notification' in window && Notification.permission !== 'denied') {\n        Notification.requestPermission().then(permission => {\n            if (permission === 'granted') {\n                console.log('Notification permission granted');\n            } else {\n                console.log('Notification permission denied');\n            }\n        });\n    }\n}\n\n//# sourceURL=webpack://npm/./src/messageNotifications.js?");

/***/ }),

/***/ "./src/relayMessenger.js":
/*!*******************************!*\
  !*** ./src/relayMessenger.js ***!
  \*******************************/
/***/ ((__unused_webpack_module, __unused_webpack_exports, __webpack_require__) => {

eval("// Add imported npm packages to this object for access in scripts\nwindow.relayMessenger = {\n    subtleCrypto: __webpack_require__(/*! ./subtleCrypto */ \"./src/subtleCrypto.js\"),\n    blazorDom: __webpack_require__(/*! ./blazorDom */ \"./src/blazorDom.js\"),\n    messageNotifications: __webpack_require__(/*! ./messageNotifications */ \"./src/messageNotifications.js\"),\n};\n\n\n//# sourceURL=webpack://npm/./src/relayMessenger.js?");

/***/ }),

/***/ "./src/subtleCrypto.js":
/*!*****************************!*\
  !*** ./src/subtleCrypto.js ***!
  \*****************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony export */ __webpack_require__.d(__webpack_exports__, {\n/* harmony export */   \"decrypt_AesGcm\": () => (/* binding */ decrypt_AesGcm),\n/* harmony export */   \"decrypt_RsaOaep\": () => (/* binding */ decrypt_RsaOaep),\n/* harmony export */   \"deriveKey_EcdhKeyDerive_AesKeyGen\": () => (/* binding */ deriveKey_EcdhKeyDerive_AesKeyGen),\n/* harmony export */   \"encrypt_AesGcm\": () => (/* binding */ encrypt_AesGcm),\n/* harmony export */   \"encrypt_RsaOaep\": () => (/* binding */ encrypt_RsaOaep),\n/* harmony export */   \"exportKey\": () => (/* binding */ exportKey),\n/* harmony export */   \"generateKey_EcKeyGen\": () => (/* binding */ generateKey_EcKeyGen),\n/* harmony export */   \"generateKey_RsaHashedKeyGen\": () => (/* binding */ generateKey_RsaHashedKeyGen),\n/* harmony export */   \"importKey_EcKeyImport\": () => (/* binding */ importKey_EcKeyImport)\n/* harmony export */ });\n// The function name is a compounding of the SubtleCrypto function plus it's algorithm.\n// This is because Blazor JSImport/JSExport interop won't send an object, suggest serialization for extra overhead.\n// https://github.com/mdn/dom-examples/blob/main/web-crypto/encrypt-decrypt/aes-gcm.js\n// https://mdn.github.io/dom-examples/web-crypto/encrypt-decrypt/index.html\nasync function exportKey(format, key) {\n    return {\n        key: new Uint8Array(await window.crypto.subtle.exportKey(format, key))\n    };\n}\n\n// https://developer.mozilla.org/en-US/docs/Web/API/RsaHashedKeyGenParams\nasync function generateKey_RsaHashedKeyGen(algorithmName, algorithmModulusLength, algorithmPublicExponent, algorithmHash, extractable, keyUsages) {\n    return await window.crypto.subtle.generateKey(\n        {\n            name: algorithmName,\n            modulusLength: algorithmModulusLength,\n            publicExponent: algorithmPublicExponent,\n            hash: algorithmHash,\n        }, \n        extractable, \n        keyUsages);\n}\n\n// https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/decrypt\nasync function decrypt_RsaOaep(algorithmName, privateKey, payload) {\n    // Return a byte array of plain bytes\n    return {\n        payload: new Uint8Array(\n            await window.crypto.subtle.decrypt({\n                name: algorithmName\n            }, \n            privateKey, \n            payload))\n    };\n}\n\n// https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/encrypt\nasync function encrypt_RsaOaep(algorithmName, publicKey, payload) {\n    // Return object with byte array of cipher bytes\n    return {\n        payload: new Uint8Array(\n            await window.crypto.subtle.encrypt({\n                    name: algorithmName\n                }, \n                publicKey,\n                payload))\n    };\n}\n\n// https://developer.mozilla.org/en-US/docs/Web/API/EcKeyGenParams\nasync function generateKey_EcKeyGen(algorithmName, algorithmNamedCurve, extractable, keyUsages) {\n    return await window.crypto.subtle.generateKey({\n            name: algorithmName, \n            namedCurve: algorithmNamedCurve,\n        }, \n        extractable, \n        keyUsages);\n}\n\n// https://developer.mozilla.org/en-US/docs/Web/API/EcdhKeyDeriveParams\nasync function deriveKey_EcdhKeyDerive_AesKeyGen(algorithmEcdhName, algorithmEcdhPublic, baseKey, deriveBitLength, algorithmAesName, algorithmAesLength, extractable, keyUsages, digestAlgorithm) {\n    return await window.crypto.subtle.deriveBits({\n            name: algorithmEcdhName,\n            public: algorithmEcdhPublic,\n        },\n        baseKey,\n        deriveBitLength)\n        .then((derivedBits) => window.crypto.subtle.digest(digestAlgorithm, derivedBits)\n        .then((hashedDerivedBits) => window.crypto.subtle.importKey(\n            'raw',\n            hashedDerivedBits,\n            {\n                name: algorithmAesName,\n                length: algorithmAesLength,\n            },\n            extractable,\n            keyUsages\n        )));\n}\n\n// https://developer.mozilla.org/en-US/docs/Web/API/EcKeyImportParams\nasync function importKey_EcKeyImport(format, keyData, algorithmName, algorithmNamedCurve, extractable, keyUsages) {\n    return await window.crypto.subtle.importKey(\n        format,\n        keyData,\n        {\n            name: algorithmName,\n            namedCurve: algorithmNamedCurve,\n        },\n        extractable,\n        keyUsages);\n}\n\n// https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/decrypt\nasync function decrypt_AesGcm(algorithmName, algorithmIv, algorithmTagLength, key, payload) {\n    // Return a byte array of plain bytes\n    return {\n        payload: new Uint8Array(\n            await window.crypto.subtle.decrypt({\n                    name: algorithmName,\n                    iv: algorithmIv,\n                    tagLength: algorithmTagLength,\n                },\n                key,\n                payload))\n    };\n}\n\n// https://developer.mozilla.org/en-US/docs/Web/API/AesGcmParams\nasync function encrypt_AesGcm(algorithmName, algorithmIv, algorithmTagLength, key, payload) {\n    // Return object with byte array of cipher bytes\n    return {\n        payload: new Uint8Array(\n            await window.crypto.subtle.encrypt({\n                    name: algorithmName,\n                    iv: algorithmIv,\n                    tagLength: algorithmTagLength,\n                },\n                key,\n                payload))\n    };\n}\n\n\n//# sourceURL=webpack://npm/./src/subtleCrypto.js?");

/***/ })

/******/ 	});
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId](module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/define property getters */
/******/ 	(() => {
/******/ 		// define getter functions for harmony exports
/******/ 		__webpack_require__.d = (exports, definition) => {
/******/ 			for(var key in definition) {
/******/ 				if(__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
/******/ 					Object.defineProperty(exports, key, { enumerable: true, get: definition[key] });
/******/ 				}
/******/ 			}
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/hasOwnProperty shorthand */
/******/ 	(() => {
/******/ 		__webpack_require__.o = (obj, prop) => (Object.prototype.hasOwnProperty.call(obj, prop))
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/make namespace object */
/******/ 	(() => {
/******/ 		// define __esModule on exports
/******/ 		__webpack_require__.r = (exports) => {
/******/ 			if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 				Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 			}
/******/ 			Object.defineProperty(exports, '__esModule', { value: true });
/******/ 		};
/******/ 	})();
/******/ 	
/************************************************************************/
/******/ 	
/******/ 	// startup
/******/ 	// Load entry module and return exports
/******/ 	// This entry module can't be inlined because the eval devtool is used.
/******/ 	var __webpack_exports__ = __webpack_require__("./src/relayMessenger.js");
/******/ 	
/******/ })()
;
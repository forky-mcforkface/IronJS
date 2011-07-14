// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
* @name: S15.5.4.14_A3_T7;
* @section: 15.5.4.14;
* @assertion: String.prototype.split() returns an Array object with:
*  i) length equaled to 1,
* ii) [[Get]](0) equaled to the result of converting this object to a string;
* @description: Instance is Math;
*/


// Converted for Test262 from original Sputnik source

ES5Harness.registerTest( {
id: "S15.5.4.14_A3_T7",

path: "TestCases/15_Native/15.5_String_Objects/15.5.4_Properties_of_the_String_Prototype_Object/15.5.4.14_String.prototype.split/S15.5.4.14_A3_T7.js",

assertion: "String.prototype.split() returns an Array object with:",

description: "Instance is Math",

test: function testcase() {
   var __instance = Math;

__instance.split = String.prototype.split;

var __split = __instance.split();

//////////////////////////////////////////////////////////////////////////////
//CHECK#1
if (__split.constructor !== Array) {
  $ERROR('#1: var __instance = Math; __instance.split = String.prototype.split; __split = __instance.split(); __split.constructor === Array. Actual: '+__split.constructor );
}
//
//////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////
//CHECK#2
if (__split.length !== 1) {
  $ERROR('#2: var __instance = Math; __instance.split = String.prototype.split; __split = __instance.split(); __split.length === 1. Actual: '+__split.length );
}
//
//////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////
//CHECK#3
if (__split[0] !== "[object Math]") {
  $ERROR('#3: var __instance = Math; __instance.split = String.prototype.split; __split = __instance.split(); __split[0] === "[object Math]". Actual: '+__split[0] );
}
//
//////////////////////////////////////////////////////////////////////////////

 }
});

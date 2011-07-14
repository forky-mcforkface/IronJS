// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
* @name: S15.5.4.15_A1_T4;
* @section: 15.5.4.15;
* @assertion: String.prototype.substring (start, end);
* @description: Arguments are null and number, and instance is function call, that returned string;
*/


// Converted for Test262 from original Sputnik source

ES5Harness.registerTest( {
id: "S15.5.4.15_A1_T4",

path: "TestCases/15_Native/15.5_String_Objects/15.5.4_Properties_of_the_String_Prototype_Object/15.5.4.15_String.prototype.substring/S15.5.4.15_A1_T4.js",

assertion: "String.prototype.substring (start, end)",

description: "Arguments are null and number, and instance is function call, that returned string",

test: function testcase() {
   //////////////////////////////////////////////////////////////////////////////
//CHECK#1
if (function(){return "gnulluna"}().substring(null, -3) !== "") {
  $ERROR('#1: function(){return "gnulluna"}().substring(null, -3) === "". Actual: '+function(){return "gnulluna"}().substring(null, -3) );
}
//
//////////////////////////////////////////////////////////////////////////////

 }
});

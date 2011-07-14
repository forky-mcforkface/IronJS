// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
 * @name: S15.1.2.1_A4.1;
 * @section: 15.1.2.1, 15.2.4.7, 12.6.4;
 * @assertion: The length property of eval has the attribute DontEnum;
 * @description: Checking use propertyIsEnumerable, for-in;
*/



// Converted for Test262 from original Sputnik source

ES5Harness.registerTest( {
id: "S15.1.2.1_A4.1",

path: "TestCases/15_Native/15.1_The_Global_Object/15.1.2_Function_Properties_of_the_Global_Object/15.1.2.1_eval/S15.1.2.1_A4.1.js",

assertion: "The length property of eval has the attribute DontEnum",

description: "Checking use propertyIsEnumerable, for-in",

test: function testcase() {
   //CHECK#1
if (eval.propertyIsEnumerable('length') !== false) {
  $ERROR('#1: eval.propertyIsEnumerable(\'length\') === false. Actual: ' + (eval.propertyIsEnumerable('length')));
}

//CHECK#2
var result = true;
for (p in eval){
  if (p === "length") {
    result = false;
  }  
}

if (result !== true) {
  $ERROR('#2: result = true; for (p in eval) { if (p === "length") result = false; };  result === true;');
}

 }
});

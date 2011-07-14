// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
* @name: S11.4.2_A2_T1;
* @section: 11.4.2;
* @assertion: Operator "void" uses GetValue;
* @description: Either Type(x) is not Reference or GetBase(x) is not null;
*/


// Converted for Test262 from original Sputnik source

ES5Harness.registerTest( {
id: "S11.4.2_A2_T1",

path: "TestCases/11_Expressions/11.4_Unary_Operators/11.4.2_The_void_Operator/S11.4.2_A2_T1.js",

assertion: "Operator \"void\" uses GetValue",

description: "Either Type(x) is not Reference or GetBase(x) is not null",

test: function testcase() {
   //CHECK#1
if (void 0 !== undefined) {
  $ERROR('#1: void 0 === undefined. Actual: ' + (void 0));
}

//CHECK#2
var x = 0;
if (void x !== undefined) {
  $ERROR('#2: var x = 0; void x === undefined. Actual: ' + (void x));
}

//CHECK#3
var x = new Object();
if (void x !== undefined) {
  $ERROR('#3: var x = new Object(); void x === undefined. Actual: ' + (void x));
}

 }
});

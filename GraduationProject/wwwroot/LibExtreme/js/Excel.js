﻿/*! ExcelJS 11-02-2020 */

!function (e) {
    if ("object" == typeof exports && "undefined" != typeof module) module.exports = e();
    else if ("function" == typeof define && define.amd) define([], e); else { ("undefined" != typeof window ? window : "undefined" != typeof global ? global : "undefined" != typeof self ? self : this).ExcelJS = e() }
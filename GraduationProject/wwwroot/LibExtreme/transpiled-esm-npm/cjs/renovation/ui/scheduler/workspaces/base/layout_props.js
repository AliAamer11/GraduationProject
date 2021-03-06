"use strict";

exports.LayoutProps = void 0;
var LayoutProps = Object.defineProperties({
  leftVirtualCellWidth: 0,
  rightVirtualCellWidth: 0,
  topVirtualRowHeight: 0,
  bottomVirtualRowHeight: 0,
  addDateTableClass: true,
  addVerticalSizesClassToRows: true
}, {
  viewData: {
    get: function get() {
      return {
        groupedData: [],
        leftVirtualCellCount: 0,
        rightVirtualCellCount: 0,
        topVirtualRowCount: 0,
        bottomVirtualRowCount: 0
      };
    },
    configurable: true,
    enumerable: true
  }
});
exports.LayoutProps = LayoutProps;
"use strict";

exports.getBoundaryProps = getBoundaryProps;
exports.isReachedBottom = isReachedBottom;
exports.isReachedLeft = isReachedLeft;
exports.isReachedRight = isReachedRight;
exports.isReachedTop = isReachedTop;

var _get_scroll_left_max = require("./get_scroll_left_max");

var _get_scroll_top_max = require("./get_scroll_top_max");

var _scroll_direction = require("./scroll_direction");

function isReachedLeft(scrollOffsetLeft, epsilon) {
  return Math.round(scrollOffsetLeft) <= epsilon;
}

function isReachedRight(element, scrollOffsetLeft, epsilon) {
  return Math.round((0, _get_scroll_left_max.getScrollLeftMax)(element) - scrollOffsetLeft) <= epsilon;
}

function isReachedTop(scrollOffsetTop, epsilon) {
  return Math.round(scrollOffsetTop) <= epsilon;
}

function isReachedBottom(element, scrollOffsetTop, pocketHeight, epsilon) {
  return Math.round((0, _get_scroll_top_max.getScrollTopMax)(element) - scrollOffsetTop - pocketHeight) <= epsilon;
}

function getBoundaryProps(direction, scrollOffset, element) {
  var pocketHeight = arguments.length > 3 && arguments[3] !== undefined ? arguments[3] : 0;
  var left = scrollOffset.left,
      top = scrollOffset.top;
  var boundaryProps = {};

  var _ScrollDirection = new _scroll_direction.ScrollDirection(direction),
      isHorizontal = _ScrollDirection.isHorizontal,
      isVertical = _ScrollDirection.isVertical;

  if (isHorizontal) {
    boundaryProps.reachedLeft = isReachedLeft(left, 0);
    boundaryProps.reachedRight = isReachedRight(element, left, 0);
  }

  if (isVertical) {
    boundaryProps.reachedTop = isReachedTop(top, 0);
    boundaryProps.reachedBottom = isReachedBottom(element, top, pocketHeight, 0);
  }

  return boundaryProps;
}
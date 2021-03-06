import _objectWithoutPropertiesLoose from "@babel/runtime/helpers/esm/objectWithoutPropertiesLoose";
import _extends from "@babel/runtime/helpers/esm/extends";
var _excluded = ["overflowIndicatorTemplate", "viewModel"];
import { createComponentVNode, normalizeProps } from "inferno";
import { BaseInfernoComponent, normalizeStyles } from "@devextreme/runtime/inferno";
import { combineClasses } from "../../../../utils/combine_classes";
import { Button } from "../../../button";
import { getOverflowIndicatorStyles } from "./utils";
import messageLocalization from "../../../../../localization/message";
export var viewFunction = _ref => {
  var {
    appointmentCount,
    classes,
    isCompact,
    props: {
      overflowIndicatorTemplate: OverflowIndicatorTemplate
    },
    styles,
    text
  } = _ref;
  return createComponentVNode(2, Button, {
    "text": text,
    "style": normalizeStyles(styles),
    "className": classes,
    "type": "default",
    "stylingMode": "contained",
    children: OverflowIndicatorTemplate && OverflowIndicatorTemplate({
      appointmentCount: appointmentCount,
      isCompact: isCompact
    })
  });
};
export var OverflowIndicatorProps = {};

var getTemplate = TemplateProp => TemplateProp && (TemplateProp.defaultProps ? props => normalizeProps(createComponentVNode(2, TemplateProp, _extends({}, props))) : TemplateProp);

export class OverflowIndicator extends BaseInfernoComponent {
  constructor(props) {
    super(props);
    this.state = {};
  }

  get appointmentCount() {
    return this.props.viewModel.items.settings.length;
  }

  get isCompact() {
    return this.props.viewModel.isCompact;
  }

  get text() {
    var {
      isCompact
    } = this.props.viewModel;

    if (isCompact) {
      return "".concat(this.appointmentCount);
    }

    var formatter = messageLocalization.getFormatter("dxScheduler-moreAppointments");
    return formatter(this.appointmentCount);
  }

  get styles() {
    return getOverflowIndicatorStyles(this.props.viewModel);
  }

  get classes() {
    return combineClasses({
      "dx-scheduler-appointment-collector": true,
      "dx-scheduler-appointment-collector-compact": this.isCompact
    });
  }

  get restAttributes() {
    var _this$props = this.props,
        restProps = _objectWithoutPropertiesLoose(_this$props, _excluded);

    return restProps;
  }

  render() {
    var props = this.props;
    return viewFunction({
      props: _extends({}, props, {
        overflowIndicatorTemplate: getTemplate(props.overflowIndicatorTemplate)
      }),
      appointmentCount: this.appointmentCount,
      isCompact: this.isCompact,
      text: this.text,
      styles: this.styles,
      classes: this.classes,
      restAttributes: this.restAttributes
    });
  }

}
OverflowIndicator.defaultProps = OverflowIndicatorProps;
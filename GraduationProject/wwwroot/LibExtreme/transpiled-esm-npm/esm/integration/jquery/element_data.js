// eslint-disable-next-line no-restricted-imports
import jQuery from 'jquery';
import { setDataStrategy } from '../../core/element_data';
import useJQueryFn from './use_jquery';
var useJQuery = useJQueryFn();

if (useJQuery) {
  setDataStrategy(jQuery);
}
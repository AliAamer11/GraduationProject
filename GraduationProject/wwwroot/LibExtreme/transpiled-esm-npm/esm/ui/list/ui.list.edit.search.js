import ListEdit from './ui.list.edit';
import searchBoxMixin from '../widget/ui.search_box_mixin';
var ListSearch = ListEdit.inherit(searchBoxMixin).inherit({
  _addWidgetPrefix: function _addWidgetPrefix(className) {
    return 'dx-list-' + className;
  },
  _getCombinedFilter: function _getCombinedFilter() {
    var filter;
    var storeLoadOptions;
    var dataSource = this._dataSource;

    if (dataSource) {
      storeLoadOptions = {
        filter: dataSource.filter()
      };

      dataSource._addSearchFilter(storeLoadOptions);

      filter = storeLoadOptions.filter;
    }

    return filter;
  },
  _initDataSource: function _initDataSource() {
    var value = this.option('searchValue');
    var expr = this.option('searchExpr');
    var mode = this.option('searchMode');
    this.callBase();

    if (this._dataSource) {
      value && value.length && this._dataSource.searchValue(value);
      mode.length && this._dataSource.searchOperation(searchBoxMixin.getOperationBySearchMode(mode));
      expr && this._dataSource.searchExpr(expr);
    }
  }
});
export default ListSearch;
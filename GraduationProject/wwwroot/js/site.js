// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


$(function () {

    var PlaceHolderElement = $('#PlaceHolderHer');

    $('button[ data-toggle="ajax-modal"]').click(function (event) {
        //var element = document.getElementsByClassName("modal-backdrop");
        //element.style.backgroundColor = 'yellow';
        var url = $(this).data('url');
        $.get(url).done(function (data) {
            PlaceHolderElement.html(data);
            PlaceHolderElement.find('.modal').modal('show');
        })
    })

    PlaceHolderElement.on('click','[data-save="modal"]', function (event) {

        var myform = $(this).parents('.modal').find('form');
        var actionUrl = myform.attr('action');
        var sendData = myform.serialize();
        $.post(actionUrl,sendData).done(function (data) {
            PlaceHolderElement.find('.modal').modal('hide');
            location.reload(true);
        })
    })
})
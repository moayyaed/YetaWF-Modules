/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ImageRepository#License */

var YetaWF_ImageRepository = {};

YetaWF_ImageRepository.initSelection = function (divId) {

    var $control = $('#' + divId);
    if ($control.length != 1) throw "div not found";/*DEBUG*/

    var $hidden = $('input[type="hidden"]', $control);
    if ($hidden.length != 1) throw "hidden field not found";/*DEBUG*/

    var $list = $('select[name="List"]', $control);
    if ($list.length != 1) throw "list not found";/*DEBUG*/

    var $img = $('.t_preview img', $control);
    if ($img.length != 1) throw "preview image not found";/*DEBUG*/

    var $buttons = $('.t_haveimage', $control);
    if ($buttons.length != 1) throw "button area not found";/*DEBUG*/

    // set the preview image
    function setPreview(name) {
        $buttons.toggle(name!=null && name.length > 0);
        var src = $img.attr('src');
        var currUri = $YetaWF.parseUrl(src);
        currUri.removeSearch("Name");
        currUri.addSearch("Name", name);
        $img.attr('src', currUri.toUrl());
    }
    function clearFileName() {
        $hidden.val('');
        $list.val('');
        setPreview('');
        $('.t_haveimage', $control).toggle(false);
    }
    function successfullUpload(js) {
        $hidden.val(js.filename);
        $list.html(js.list);
        $list.val(js.filename);
        setPreview(js.filename);
        $('.t_haveimage', $control).toggle(js.filename.length > 0);
    }
    // set upload control settings (optional)
    var $upload = $('.yt_fileupload1', $control);
    if ($upload.length == 1) {
        $upload.data({
            getFileName: undefined, // we handle deleting the file
            successfullUpload: successfullUpload,
        });
    }
    // handle the clear button
    $('#' + divId + ' a[data-name="Clear"]').click(function (e) {
        e.preventDefault();
        clearFileName();
        return false;
    });
    // handle the remove button
    $('#' + divId + ' a[data-name="Remove"]').click(function (e) {
        e.preventDefault();
        var $this = $(this);

        // get url to remove the file
        var href = $this.attr('href');
        var uri = $YetaWF.parseUrl(href);
        uri.removeSearch("Name");
        uri.addSearch("Name", $hidden.val());

        $.ajax({
            url: uri.toUrl(),
            type: 'post',
            success: function (result, textStatus, jqXHR) {
                if (result.startsWith(YConfigs.Basics.AjaxJavascriptReturn)) {
                    var script = result.substring(YConfigs.Basics.AjaxJavascriptReturn.length);
                    eval(script);
                    return;
                } else if (result.startsWith(YConfigs.Basics.AjaxJavascriptErrorReturn)) {
                    var script = result.substring(YConfigs.Basics.AjaxJavascriptErrorReturn.length);
                    eval(script);
                    return;
                }
                var js = JSON.parse(result);
                eval(js.result);
                $list.html(js.list);
                clearFileName();
                return false;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $YetaWF.alert(YLocs.Forms.AjaxError.format(jqXHR.status, jqXHR.statusText), YLocs.Forms.AjaxErrorTitle);
                return false;
            }
        });
        return false;
    });

    // user changed the selected image
    $('#' + divId + ' select[name="List"]').change(function () {
        var $this = $(this);
        var name = $this.val();
        setPreview(name);
        $hidden.val(name);
    });
    // show initial selection (if any)
    $list.val($hidden.val());
    $list.trigger('change');
};


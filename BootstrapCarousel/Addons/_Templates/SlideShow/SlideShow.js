﻿/* Copyright © 2017 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/BootstrapCarousel#License */

var YetaWF_BootstrapCarousel = {};

YetaWF_BootstrapCarousel.init = function (divId) {
    'use strict';

    var TEMPLATENAME = 'YetaWF_BootstrapCarousel_SlideShow';
    var $control = $('#' + divId);
    if ($control.length != 1) throw "div not found";/*DEBUG*/

    // disable the first << button
    $('input.t_up', $control).filter(":first").attr("disabled", "disabled");
    // disable the last >> button
    $('input.t_down', $control).filter(":last").attr("disabled", "disabled");
    // disable Remove button if there is only one image
    var $dels = $('input.t_delete', $control);
    if ($dels.length <= 1)
        $dels.attr("disabled", "disabled");

    function getSlideIndex(obj) {
        var $obj = $(obj);
        var $slide = $obj.closest('.t_slide');
        if ($slide.length != 1) throw "Can't find slide";/*DEBUG*/
        var index = $('input[name$=".Index"]', $slide).val();
        if (index == undefined) throw "Can't find slide index (hidden input)";/*DEBUG*/
        return index;
    }

    // << button click
    $('input.t_up', $control).on('click', function () {
        var slideIndex = getSlideIndex(this);
        YetaWF_Forms.submitTemplate(this, true, TEMPLATENAME, YConfigs.YetaWF_BootstrapCarousel.Action_MoveLeft, slideIndex);
    });
    // >> button click
    $('input.t_down', $control).on('click', function () {
        var slideIndex = getSlideIndex(this);
        YetaWF_Forms.submitTemplate(this, true, TEMPLATENAME, YConfigs.YetaWF_BootstrapCarousel.Action_MoveRight, slideIndex);
    });
    // delete button click
    $dels.on('click', function () {
        var btn = this;
        Y_AlertYesNo(YLocs.YetaWF_BootstrapCarousel.RemoveConfirm, YLocs.YetaWF_BootstrapCarousel.RemoveTitle, function () {
            var slideIndex = getSlideIndex(btn);
            YetaWF_Forms.submitTemplate(btn, true, TEMPLATENAME, YConfigs.YetaWF_BootstrapCarousel.Action_Remove, slideIndex);
        });
    });
    // Insert button click
    $('input.t_ins', $control).on('click', function () {
        var slideIndex = getSlideIndex(this);
        YetaWF_Forms.submitTemplate(this, true, TEMPLATENAME, YConfigs.YetaWF_BootstrapCarousel.Action_Insert, slideIndex);
    });
    // Add button click
    $('input.t_add', $control).on('click', function () {
        var slideIndex = getSlideIndex(this);
        YetaWF_Forms.submitTemplate(this, true, TEMPLATENAME, YConfigs.YetaWF_BootstrapCarousel.Action_Add, slideIndex);
    });
};


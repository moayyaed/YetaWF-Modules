﻿/* Copyright © 2017 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Text#License */

$(document).ready(function () {
    $('body').on('click', '.YetaWF_Text .FAQ_Q', function(e) {
        var $this = $(this);
        $this.next('.FAQ_A').toggle();
    })
});
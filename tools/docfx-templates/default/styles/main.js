// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.
$(function() {
    $('img[alt=screenshot]').click(function(){
        $('#lightbox').html($(this)[0].outerHTML);
        $('#lightbox').toggleClass('visible');
    });

    $('#lightbox').click(function(){
        $('#lightbox').toggleClass('visible');
    });
});

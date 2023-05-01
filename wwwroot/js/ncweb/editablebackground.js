if (window.nceditablebackground == null) {
    window.nceditablebackground = {};
}

window.nceditablebackground.mixin = function (vueModelInstance, pageId) {

    var currentMounted = vueModelInstance.mounted;
    var editorMounted = function () {

        $('head').append('<link rel="stylesheet" type="text/css" href="/css/ncweb/editablebackground.min.css">');

        var $input = $('<input type="file" class="nceditablebackground_uploader" accept="image/*" hidden>');
        $input.appendTo('body');

        $("*[ncweb-editablebackground]").each(function () {

            var $me = $(this);
            var element = this;
            var count = 0;

            $('<button class="ncwebeditablebackground_button"></button>')
                .click(async function () {

                    $input[0].onchange = async function () {

                        const clickFile = this.files[0];
                        if (clickFile == null) {
                            return;
                        }

                        const formData = new FormData();
                        formData.append('file', clickFile)
                        formData.append('target', $me.attr("ncweb-editablebackground"));

                        const options = {
                            method: 'POST',
                            body: formData,
                        };

                        try {
                            await fetch('/__editableimage/upload', options);
                        }
                        catch {

                        }

                        count++;
                        $me.css("background-image", `url(/'${$me.attr("ncweb-editablebackground")}?${count}')`);
                    };

                    $input.click();
                })
                .appendTo($me);

        });
    }

    vueModelInstance.mounted = function () {
        currentMounted?.();
        editorMounted();
    }

    return vueModelInstance;
}
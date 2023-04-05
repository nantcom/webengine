if (window.nceditor == null) {
    window.nceditor = {};
}

window.nceditor.editormixin = function (vueModelInstance) {

    var currentMounted = vueModelInstance.mounted;
    var editorMounted = function () {

        $('head').append('<link rel="stylesheet" type="text/css" href="/css/ncweb/editor.css">');

        $("*[ncweb-contentpart]").each(function () {
            var me = $(this);
            var element = this;

            // edit image
            $('<div class="ncweb_button edit">').click(function () {

                element.contentEditable = true;
            }).appendTo(me);

            $('<div class="ncweb_button save">').click(async function () {

                var html = me.html();

                await window.ncvuesync.callServer("NC.WebEngine.Core.Editor.EditorVueModel", "SavePart",
                    {
                        content: html,
                        partName: me.attr("ncweb-contentpart"),
                        pageUrl: window.location.pathname
                    });
                element.contentEditable = false;

            }).appendTo(me);
        });
    }

    vueModelInstance.mounted = function () {
        currentMounted?.();
        editorMounted();
    }

    return vueModelInstance;
}
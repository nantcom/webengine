if (window.nceditor == null) {
    window.nceditor = {};
}

window.nceditor.editormixin = function (vueModelInstance, pageId) {

    if (pageId == null) {
        throw "missing pageId";
    }

    var currentMounted = vueModelInstance.mounted;
    var editorMounted = function () {

        $('head').append('<link rel="stylesheet" type="text/css" href="/css/ncweb/editor.css">');

        var $savedCheck = $('<div class="ncweb_saved"></div>');
        $savedCheck.appendTo('body');

        $("*[ncweb-contentpart]").each(function () {
            var me = $(this);
            var element = this;

            // edit image
            $('<div class="ncweb_button edit">').click(function () {

                element.contentEditable = true;
            }).appendTo(me);

            $('<div class="ncweb_button save">').click(async function () {

                var html = element.outerHTML;

                var $tempDom = $(html);
                $tempDom.children(".ncweb_button").remove();

                html = $tempDom.html();

                await window.ncvuesync.callServer("NC.WebEngine.Core.Editor.EditorVueModel", "SavePart",
                    {
                        Language: null,
                        Name: me.attr("ncweb-contentpart"),
                        ContentPageId: pageId,
                        Content: html,
                        IsBlockContent: false
                    });

                $savedCheck.addClass("show").delay(2000).queue(function (next) {
                    $(this).removeClass('show');
                        next();
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
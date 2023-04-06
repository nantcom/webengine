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
            var oldContent = me.html();

            var myPageId = pageId;
            if (me.attr("ncweb-contentpageid") != null) {
                myPageId = me.attr("ncweb-contentpageid");
            }

            var editButton = $('<div class="ncweb_button edit">');

            var handleEdit = function () {

                element.contentEditable = true;
                element.addEventListener('keydown', function (event) {
                    if (event.code !== 'Space') {
                        return
                    }
                    event.preventDefault()
                    document.execCommand("insertText", false, ' ')
                })

                var saveButton = $('<div class="ncweb_floatingbutton save">');
                var cancelButton = $('<div class="ncweb_floatingbutton cancel">');

                saveButton.click(async function () {

                    var html = me.html();

                    await window.ncvuesync.callServer("NC.WebEngine.Core.Editor.EditorVueModel", "SavePart",
                        {
                            Language: null,
                            Name: me.attr("ncweb-contentpart"),
                            ContentPageId: parseInt(myPageId),
                            Content: html,
                            IsBlockContent: false
                        });

                    $savedCheck.addClass("show").delay(2000).queue(function (next) {
                        $(this).removeClass('show');
                        next();
                    });

                    cancelButton.remove();
                    saveButton.remove();

                    oldContent = html;
                    element.contentEditable = false;

                    editButton.appendTo(me);
                    editButton.click(handleEdit);

                }).appendTo('body');

                cancelButton.click(async function () {

                    cancelButton.remove();
                    saveButton.remove();

                    element.contentEditable = false;

                    me.html(oldContent);
                    editButton.appendTo(me);
                    editButton.click(handleEdit);

                }).appendTo('body');

                editButton.remove();
            };

            // edit image
            editButton.appendTo(me);
            editButton.click(handleEdit);

        });
    }

    vueModelInstance.mounted = function () {
        currentMounted?.();
        editorMounted();
    }

    return vueModelInstance;
}
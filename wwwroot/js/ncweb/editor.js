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
            var myContentPartId = 0;

            if (element.hasAttribute("readonly")) {
                element.removeAttribute("ncweb-contentpart");
                return;
            }

            var myPageId = pageId;
            if (me.attr("ncweb-contentpageid") != null) {
                myPageId = me.attr("ncweb-contentpageid");
            }

            var editButton = $('<div class="ncweb_button edit">');

            var handleEdit = function () {

                var currentButton = $(this);
                $(".ncweb_button.edit").css("visibility", "hidden");
                currentButton.css("visibility", "visible");

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

                    // on first save, we will get the content part id
                    // if user save again in same edit seession, we will save to same id
                    var result = await window.ncvuesync.callServer("NC.WebEngine.Core.Editor.EditorVueModel", "SavePart",
                        {
                            Id: myContentPartId,
                            Language: null,
                            Name: me.attr("ncweb-contentpart"),
                            ContentPageId: parseInt(myPageId),
                            Content: html,
                            IsBlockContent: false
                        });

                    myContentPartId = result.data.Id;

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

                    $(".ncweb_button.edit").css("visibility", "visible");

                }).appendTo('body');

                cancelButton.click(async function () {

                    cancelButton.remove();
                    saveButton.remove();

                    element.contentEditable = false;

                    me.html(oldContent);
                    editButton.appendTo(me);
                    editButton.click(handleEdit);

                    $(".ncweb_button.edit").css("visibility", "visible");

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
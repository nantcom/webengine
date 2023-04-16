if (window.nceditablecollection == null) {
    window.nceditablecollection = {};
}

window.nceditablecollection.mixin = function (vueModelInstance, pageId) {

    if (pageId == null) {
        throw "missing pageId";
    }


    var currentMounted = vueModelInstance.mounted;
    var editorMounted = function () {

        $('head').append('<link rel="stylesheet" type="text/css" href="/css/ncweb/editablecollection.min.css">');

        $("*[ncweb-editablecollection] *[ncweb-collectionitem]").each(function () {

            var $me = $(this);
            var element = this;

            $('<button class="ncwebeditablecollection_button remove"></button>')
                .click(async function () {

                    if (element.hasAttribute("itemid") == false) {
                        alert("Missing Item Id");
                        return;
                    }

                    if (confirm(`Moving this item to recycle bin?`) == false) {
                        return;
                    }

                    await window.ncvuesync.callServer("NC.WebEngine.Core.EditableCollection.EditableCollectionVueModel",
                        "DeletePage", parseInt($me.attr("itemid")));

                    window.location.reload();

                })
            .appendTo($me);
        });

        $("*[ncweb-editablecollection]").each(function () {

            var $me = $(this);
            var element = this;
            var parent = $me.attr("ncweb-editablecollection");

            $('<button class="ncwebeditablecollection_button add"></button>')
                .click(async function () {

                    if (confirm(`Create new item under ${parent}?`) == false) {
                        return;
                    }

                    await window.ncvuesync.callServer("NC.WebEngine.Core.EditableCollection.EditableCollectionVueModel", "CreatePage", parent);

                    window.location.reload();

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
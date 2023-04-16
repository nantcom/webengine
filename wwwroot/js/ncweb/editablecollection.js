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


        $("*[ncweb-editablecollection] *[ncweb-itemdate]").each(function () {

            var $me = $(this);
            var element = this;
            var currentValue = $me.attr("ncweb-itemdate");
            var itemId = $me.parents("[itemid]").attr("itemid");

            var valueAsDate = new Date(parseInt(currentValue));
            valueAsDate.setMinutes(valueAsDate.getMinutes() - valueAsDate.getTimezoneOffset())

            var curretnValueString = valueAsDate.toISOString().slice(0, 19);

            var rebuildInput = function () {

                $('<input class="picker" type="datetime-local">')
                    .change(async function () {

                        var newValue = $(this)[0].value;

                        var result = await window.ncvuesync.callServer("NC.WebEngine.Core.EditableCollection.EditableCollectionVueModel",
                            "SetPageDate",
                            {
                                pageId: parseInt(itemId),
                                outputFormat: $me.attr("ncweb-itemdateformat"),
                                date: newValue
                            });

                        if (result.isSuccess) {

                            curretnValueString = newValue;
                            $me.text(result.data);
                            rebuildInput();

                            alert("Value was set, refresh the page to see new items rearranged");
                        }

                    })
                    .attr("value", curretnValueString)
                    .appendTo($me);
            };
            rebuildInput();
        });
    }

    vueModelInstance.mounted = function () {
        currentMounted?.();
        editorMounted();
    }

    return vueModelInstance;
}
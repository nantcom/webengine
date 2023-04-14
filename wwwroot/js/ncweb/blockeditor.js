if (window.ncblockeditor == null) {
    window.ncblockeditor = {};
}

window.ncblockeditor.mixin = function (vueModelInstance, pageId) {

    if (pageId == null) {
        throw "missing pageId";
    }

    var currentMounted = vueModelInstance.mounted;
    var editorMounted = function () {

        $('head').append('<link rel="stylesheet" type="text/css" href="/css/ncweb/editor.css">');

        $("*[ncweb-blockcontent]").each(function () {

            var me = $(this);
            var element = this;
            var myPageId = pageId;
            var editor = null;
            var lastContent = JSON.stringify(window.ncblockeditor.data);
            var contentPart =
            {
                Id: 0,
                Language: null,
                Name: "Block",
                ContentPageId: parseInt(myPageId),
                Content: {},
                IsBlockContent: true
            };


            var saveBlock = async function () {

                var result = await editor.save();

                var current = JSON.stringify(result);
                if (current == lastContent) {
                    return;
                }

                me.addClass("notsaved");
                contentPart.Content = JSON.stringify(result);

                var result = await window.ncvuesync.callServer("NC.WebEngine.Core.Editor.EditorVueModel", "SavePart", contentPart);
                contentPart.Id = result.data.Id;

                me.addClass("saved").delay(1000).queue(function (next) {
                    me.removeClass('saved');
                    me.removeClass('notsaved');
                    next();
                });

                lastContent = current;

            };

            var saveBlockDebouncer = $.debounce(1500, saveBlock);

            me.attr("id", "editorjs");
            editor = new EditorJS({
                placeholder: 'Let`s write an awesome story!',
                holderId: 'editorjs',
                tools: {
                    header: Header,
                    code: CodeTool,
                    embed: Embed,
                    Color: {
                        class: ColorPlugin,
                        config: {
                            colorCollections: ['#EC7878', '#9C27B0', '#673AB7', '#3F51B5', '#0070FF', '#03A9F4', '#00BCD4', '#4CAF50', '#8BC34A', '#CDDC39', '#FFF'],
                            defaultColor: '#FF1300',
                            type: 'text',
                            customPicker: true // add a button to allow selecting any colour  
                        }
                    },
                    Marker: {
                        class: ColorPlugin,
                        config: {
                            defaultColor: '#FFBF00',
                            type: 'marker',
                            icon: `<svg fill="#000000" height="200px" width="200px" version="1.1" id="Icons" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 32 32" xml:space="preserve"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <g> <path d="M17.6,6L6.9,16.7c-0.2,0.2-0.3,0.4-0.3,0.6L6,23.9c0,0.3,0.1,0.6,0.3,0.8C6.5,24.9,6.7,25,7,25c0,0,0.1,0,0.1,0l6.6-0.6 c0.2,0,0.5-0.1,0.6-0.3L25,13.4L17.6,6z"></path> <path d="M26.4,12l1.4-1.4c1.2-1.2,1.1-3.1-0.1-4.3l-3-3c-0.6-0.6-1.3-0.9-2.2-0.9c-0.8,0-1.6,0.3-2.2,0.9L19,4.6L26.4,12z"></path> </g> <g> <path d="M28,29H4c-0.6,0-1-0.4-1-1s0.4-1,1-1h24c0.6,0,1,0.4,1,1S28.6,29,28,29z"></path> </g> </g></svg>`
                        }
                    },
                    table: {
                        class: Table,
                        inlineToolbar: true,
                        config: {
                            rows: 2,
                            cols: 3,
                        },
                    },
                    image: {
                        class: ImageTool,
                        config: {
                            additionalRequestData: {
                                pageId: myPageId
                            },
                            endpoints: {
                                byFile: '/__blockeditor/imageupload',
                                byUrl: '/__blockeditor/imagefetch'
                            }
                        }
                    }
                },
                onReady: () => {
                    const undo = new Undo({ editor });
                    undo.initialize(window.ncblockeditor.data);
                },
                onChange: function () {

                    me.addClass("notsaved");
                    saveBlockDebouncer();
                },
                data: window.ncblockeditor.data
            });

            me.keypress(function () {

                me.addClass("notsaved");
            });
        });
    };

    vueModelInstance.mounted = function () {
        currentMounted?.();
        editorMounted();
    }

    return vueModelInstance;
};
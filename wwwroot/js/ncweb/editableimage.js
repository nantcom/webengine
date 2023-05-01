if (window.nceditableimage == null) {
    window.nceditableimage = {};
}

window.nceditableimage.mixin = function (vueModelInstance, pageId) {

    if (pageId == null) {
        throw "missing pageId";
    }

    function readFileAsync(file) {
        return new Promise((resolve, reject) => {
            let reader = new FileReader();

            reader.onload = () => {
                resolve(reader.result);
            };

            reader.onerror = reject;
            reader.readAsDataURL(file);
        })
    }



    var currentMounted = vueModelInstance.mounted;
    var editorMounted = function () {

        $('head').append('<link rel="stylesheet" type="text/css" href="/css/ncweb/editableimage.min.css">');
        $('head').append('<link rel="stylesheet" type="text/css" href="/js/croppie/croppie.css">');

        $('<script src="/js/croppie/croppie.js"></script>').appendTo('body');

        var $savedCheck = $('<div class="nceditableimage_saved"></div>');
        $savedCheck.appendTo('body');

        var $croppieArea = $('<div id="nceditableimage_croppie"></div>');
        $croppieArea.appendTo('body');
        $croppieArea.click(function (event) {

            if (event.target != this) {
                return;
            }

            $croppieArea.html("");
            $croppieArea.removeClass("show");
        });

        var $input = $('<input type="file" class="nceditableimage_uploader" accept="image/*" hidden>');
        $input.appendTo('body');


        $("[ncweb-editableimage]").each(function () {
            
            var $me = $(this);
            var img = this;

            if (img.tagName != "IMG") {

                var img = $me.find("img[ncweb-editableimage-placeholder]").first();
                if (img.length != 1) {
                    console.log("Skipping editable image becase no placeholder is found or more than one placeholder specified");
                    return;
                }

                img = img[0];
            }

            $me.click(function (event) {

                event.stopPropagation();
                event.preventDefault();

                var oldWidth = $me.width();
                var oldHeight = $me.height();

                $croppieArea.html('<button></button><div><img id="nceditableimage_tocrop"></div>');

                if ($me.attr("nocrop") != null) {

                    $input[0].onchange = async function () {

                        const clickFile = this.files[0];
                        if (clickFile == null) {
                            return;
                        }

                        const formData = new FormData();
                        formData.append('file', clickFile);
                        formData.append('target', $me.attr("ncweb-editableimage"));

                        const options = {
                            method: 'POST',
                            body: formData,
                        };

                        try {
                            await fetch('/__editableimage/upload', options);
                        }
                        catch {

                        }

                        $savedCheck.addClass("show").delay(2000).queue(function (next) {
                            $(this).removeClass('show');
                            next();
                        });

                        var final = await readFileAsync(clickFile);
                        img.src = final;

                    };

                }
                else {

                    $input[0].onchange = async function () {

                        const clickFile = this.files[0];
                        if (clickFile == null) {
                            return;
                        }

                        const uploaded = await readFileAsync(clickFile);
                        $("#nceditableimage_croppie").addClass("show");
                        $("#nceditableimage_tocrop").attr("src", uploaded);

                        $("#nceditableimage_tocrop").croppie({
                            enableExif: true,
                            viewport: {
                                width: oldWidth,
                                height: oldHeight,
                                type: 'square'
                            },
                            boundary: {
                                width: oldWidth + 100,
                                height: oldHeight + 100
                            }
                        });

                        $croppieArea.find('button').click(async function () {

                            var resultBlob = await $("#nceditableimage_tocrop").croppie('result', 'blob', { oldWidth, oldHeight });

                            const formData = new FormData();
                            formData.append('file', resultBlob);
                            formData.append('target', $me.attr("ncweb-editableimage"));

                            const options = {
                                method: 'POST',
                                body: formData,
                            };

                            try {
                                await fetch('/__editableimage/upload', options);
                            }
                            catch {

                            }

                            $savedCheck.addClass("show").delay(2000).queue(function (next) {
                                $(this).removeClass('show');
                                next();
                            });

                            $croppieArea.removeClass("show");
                            $croppieArea.html("");

                            var final = await readFileAsync(resultBlob);
                            img.src = final;
                        });
                    };

                }

                $input.click();

            });

        });
    }

    vueModelInstance.mounted = function () {
        currentMounted?.();
        editorMounted();
    }

    return vueModelInstance;
}
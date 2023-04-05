if (window.ncvuesync == null) {
    window.ncvuesync = {};
}

window.ncvuesync.callServer = async function (typeName, method, parameterArray) {

    try {
        var response = await fetch(`/__vuesync/${typeName}/call/${method}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(parameterArray),
        });

        var data = await response.json();

        return {
            data: data,
            isSuccess: true
        };
    }
    catch (error) {

        return {
            data: {},
            isSuccess: false
        };
    }
}


// idea:
// - generate proxy to call server method from list intead of generate by server
// - store JSON locally then

window.ncvuesync.generateVueSync = function (syncInfo) {
    /*
    syncInfo = {
        typeName: "Editor",
        syncMethods: {
            "DoSomeThing": {
                required: {
                    "PropA": true,
                    "PropB": true,
                },
                mutated: {
                    "PropC" : true
                }
            }
        },
        callableMethods: [ "Callable" ],
        model: {},
        computed: {
            "Computed" : "expression"
        }
    };*/

    var instance = {};

    instance.model = syncInfo.model;

    instance.data = function () {

        return {
            isBusy: false,
            wasSuccessful: false,
            model: syncInfo.model,
            local: {},
            alerts: [],
        }
    };

    instance.computed = {};
    /* Compute seems to be too complicated
    for (key in syncInfo.computed) {
        var expression = syncInfo.computed[key];
        instance.computed[key] = () => eval(expression);
    }*/

    instance.methods = {};

    for (key in syncInfo.syncMethods) {

        var methodInformation = syncInfo.syncMethods[key];

        instance.methods[key] = async function () {

            if (this.isBusy) {
                return;
            }

            this.isBusy = true;
            this.isError = false;

            var toSend = JSON.parse(JSON.stringify(this.model));

            for (prop in toSend) {

                if (methodInformation.required[prop] != true) {
                    delete toSend[prop];
                }
            }

            try {
                var response = await fetch(`/__vuesync/${syncInfo.typeName}/sync/${key}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(toSend),
                });

                var data = await response.json();
                
                for (prop in data) {
                    if (methodInformation.mutated[prop] == true) {
                        this.model[prop] = data[prop];
                    }
                }

                this.isBusy = false;
                this.isError = false;
            }
            catch (error) {

                this.isBusy = false;
                this.isError = true;
                this.alerts.push({
                    type: 'danger',
                    message: error
                });
            }


        };
    }

    for (key in syncInfo.callableMethods) {

        var methodInformation = syncInfo.callableMethods[key];

        instance.methods[key] = async function (parameterArray ) {

            if (this.isBusy) {
                return;
            }

            this.isBusy = true;
            this.isError = false;

            try {
                var response = await fetch(`/__vuesync/${syncInfo.typeName}/call/${key}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(parameterArray),
                });

                var data = await response.json();

                this.isBusy = false;
                this.isError = false;

                return data;
            }
            catch (error) {

                this.isBusy = false;
                this.isError = true;
                this.alerts.push({
                    type: 'danger',
                    message: error
                });

                return {};
            }


        };
    }

    return instance;
};


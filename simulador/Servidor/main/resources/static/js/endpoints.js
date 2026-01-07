$(document).ready(function () {
    var count_res = 1, count_req = 1;
    var cb_emv, cb_s71, cb_s72;
    var array_global_res = new Array();
    var array_global_req = new Array();
    var refresh_flag = true;
    var dccFlag = false;
    var typeTxn = $("option:selected", "#select_type_txn").attr("value");
    send_txn();
    interval_request_to_log();
    alertVersion();
    get_status_txn();
    get_status_emv();

    $("#btn_refresh_log").change(function () {
        if ($(this).prop("checked")) {
            refresh_flag = true;
        } else {
            refresh_flag = false;
        }
    });

    $('#modal_EMV').modal({
        backdrop: 'static',
        keyboard: false,
        show: false
    });

    $('.close_emv').click(function () {
        clean_script();
        send_script_txn();
    });

    $("#btn_send").click(function () {
        send_txn();
    });

    $("#btn_send_script").click(function () {
        send_script_txn();
        clean_script();
        $("#modal_EMV").modal("hide");

    });

    $("#btn_cancel_script").click(function () {
        clean_script();
        send_script_txn();
        $("#modal_EMV").modal("hide");

    });

    $("#select_type_txn").change(function () {
        send_txn();
    });

    $("#select_dcc_moneda").change(function () {
        send_dcc_moneda();
    });

    $("#btn_stop").change(function () {
        let state = $(this).prop('checked');
        if (state) {
            stop_txn("1");
        } else {
            stop_txn("0");
        }

    });

    $(".checkbox_key").change(function () {
        let state = $(this).prop('checked');
        let type_key = $(this).attr("data-value");
        console.log(state + " " + type_key);
        if (state) {
            send_txn_keys("1", type_key);
            verify_checked_keys($(this));
        } else {
            send_txn_keys("0", type_key);
        }
    });

    $("#btn_moto").change(function () {
        let state = $(this).prop('checked');
        if (state) {
            $("#btn_upload_keys").bootstrapToggle('disable');
            $("input:radio[name='radio_key']").attr("disabled", true);
            send_moto_txn("1");
        } else {
            $("#btn_upload_keys").bootstrapToggle('enable');
            $("input:radio[name='radio_key']").attr("disabled", false);
            send_moto_txn("0");
        }
    });

    $("#btn_qps").change(function () {
        let state = $(this).prop('checked');
        let monto = parseFloat($("#qps_value").val());
        if (state) {
            send_qps_txn(1, monto);
        } else {
            send_qps_txn(0, 0);
        }
    });

    $("#qps_value").change(function () {
        let monto = parseFloat($("#qps_value").val());
        if(monto < 1) $("#btn_qps").attr("disabled", "disabled");
        else $("#btn_qps").removeAttr("disabled");
    });

    $("#btn_download_logs").click(function () {
        download_logs();
    });
    $("#btn_see_logs").click(function () {
        see_logs();
    });

    $("#cb_emv").change(function () {
        validate_checkbox();
    });

    $("#cb_s71").change(function () {
        validate_checkbox();
    });

    $("#cb_s72").change(function () {
        validate_checkbox();
    });

    $("#table-request tbody").on("click", "tr", function () {
        console.log("TR REQUEST CLICK");
        //offRefreshLog();
        removeActiveClass("#table-request");
        removeActiveClass("#table-response");
        let count_txn = parseInt($(this).attr("data-count"));
        addReqResToTextarea(count_txn);
    });

    $("#table-response tbody").on("click", "tr", function () {
        console.log("TR RESPONSE CLICK");
        //offRefreshLog();
        removeActiveClass("#table-request");
        removeActiveClass("#table-response");
        let count_txn = parseInt($(this).attr("data-count"));
        addReqResToTextarea(count_txn);
    });

    function alertVersion() {
        $.ajax({
            async: true,
            url: "/version_project",
            method: "GET",
            success: function (data) {
                var version = "Versi\u00F3n " + data.toString();
                //alert("Antes de realizar cualquier petici\u00F3n, valide la versi\u00F3n del proyecto...\n\
                //Usted esta usando la versi\u00F3n: "+ version);
                document.getElementById("versionP").innerHTML = version;
            },
            error: function (data) {
                console.log("ERROR al cargar la version del proyecto");
            }
        });
    }

    function validate_checkbox() {
        if ($("#cb_emv").prop('checked')) {
            cb_emv = 1;
            if ($("#cb_s71").prop('checked')) {
                $("#cb_s72").attr("disabled", true);
                cb_s71 = 1;
                cb_s72 = 0;

            } else if ($("#cb_s72").prop('checked')) {
                $("#cb_s71").attr("disabled", true);
                cb_s71 = 0;
                cb_s72 = 1;
            } else {
                cb_s71 = 0;
                cb_s72 = 0;
                $("#cb_s71").attr("disabled", false);
                $("#cb_s72").attr("disabled", false);
            }
        } else {
            cb_emv = 0;
            if ($("#cb_s71").prop('checked')) {
                cb_s71 = 1;
                cb_s72 = 0;
            } else if ($("#cb_s72").prop('checked')) {
                cb_s71 = 0;
                cb_s72 = 1;
            } else {
                cb_s71 = 0;
                cb_s72 = 0;
                $("#cb_s71").attr("disabled", false);
                $("#cb_s72").attr("disabled", false);
            }
        }
    }

    function clean_script() {
        cb_emv = 0;
        cb_s71 = 0;
        cb_s72 = 0;
        $("#cb_emv").prop("checked", false);
        $("#cb_s71").prop("checked", false);
        $("#cb_s72").prop("checked", false);
        $("#cb_emv").attr("disabled", false);
        $("#cb_s71").attr("disabled", false);
        $("#cb_s72").attr("disabled", false);

    }

    function send_txn() {
        typeTxn = $("option:selected", "#select_type_txn").attr("value");


        if (typeTxn == "D11" || typeTxn == "D12") {
            $(".modenaDCC").attr("hidden", false);
            dccFlag = true;
        } else {
            if (typeTxn == "DCC00") {
                console.log("001 true");
                dccFlag = true;
            } else {
                dccFlag = false;
            }
            $(".modenaDCC").attr("hidden", true);
        }

        $.ajax({
            async: true,
            url: "send_txn",
            method: "POST",
            data: typeTxn.toString(),
            contentType: "text/plain",
            success: function (data) {
                console.log("Success send_txn");
                $("#toast_body").html("Petici&oacute;n Enviada - C&oacute;digo: " + typeTxn);
                $('#toast_txn').toast('show');
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
        send_dcc_moneda();
    }

    function send_dcc_moneda() {
        let moneda = $("option:selected", "#select_dcc_moneda").attr("value");

        let txn_dcc = {
            dccFlag: dccFlag,
            moneda: moneda
        }
        console.log("dcc_moneda: " + txn_dcc.moneda + " flag: " + txn_dcc.dccFlag);
        $.ajax({
            async: true,
            url: "send_dcc_moneda",
            method: "POST",
            data: JSON.stringify(txn_dcc),
            contentType: "application/json",
            success: function (data) {
                console.log("Success send_dcc_moneda: " + moneda + " flag: " + dccFlag);
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
    }

    function request_to_log() {
        $.ajax({
            async: true,
            url: "/request_log",
            method: "POST",
            success: function (data) {
                add_txn(data, 1); //1 - Request_Log
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
    }

    function response_to_log() {
        $.ajax({
            async: true,
            url: "/response_log",
            method: "POST",
            success: function (data) {
                add_txn(data, 2); //2 - Response_Log
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
    }

    /*===== LOG TEST =====*/

    function request_to_log_test(type_funct) {

        switch (type_funct) {
            case 1:
                $.ajax({
                    async: true,
                    url: "/request_log_test",
                    method: "POST",
                    data: count_req.toString(),
                    contentType: "text/plain",
                    success: function (data) {
                        add_txn(data, 1);
                    },
                    error: function (data) {
                        console.log("ERROR");
                    }
                });
                break;
            case 2:
                $.ajax({
                    async: true,
                    url: "/request_log_test",
                    method: "POST",
                    data: count_res.toString(),
                    contentType: "text/plain",
                    success: function (data) {
                        add_txn(data, 2);
                    },
                    error: function (data) {
                        console.log("ERROR");
                    }
                });
                break;
        }
    }

    function send_moto_txn(state) {
        $.ajax({
            async: true,
            url: "/txn_moto",
            method: "POST",
            contentType: "text/plain",
            data: state.toString(),
            success: function (data) {
                console.log("Transaction MO/TO...");
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
    }

    function send_qps_txn(state, monto) {
        var data = {state: state, monto: monto}
        $.ajax({
            async: true,
            url: "/txn_qps",
            method: "POST",
            contentType: "application/json",
            dataType: "application/json",
            data: JSON.stringify(data),
            success: function (data) {
                console.log("Transaction QPS...");
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
    }

    function send_txn_keys(state, typeKey) {
        var data_keys = {
            state: state.toString(),
            typeKey: typeKey.toString()
        };

        $.ajax({
            async: true,
            url: "/send_txn_keys",
            method: "POST",
            contentType: "application/json",
            dataType: "application/json",
            data: JSON.stringify(data_keys),
            success: function (data) {
                console.log("Transaction keys upload...");
                console.log(data);
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
    }

    function send_script_txn() {
        cb_emv, cb_s71, cb_s72;
        var data_file = {

            emv: cb_emv.toString(),
            script71: cb_s71.toString(),
            script72: cb_s72.toString()
        };

        $.ajax({
            async: true,
            url: "/send_script_txn",
            method: "POST",
            contentType: "application/json",
            dataType: 'application/json',
            data: JSON.stringify(data_file),
            success: function (data) {
                console.log("Transaction file...");
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
    }

    function stop_txn(typeTxn) {
        $.ajax({
            async: true,
            url: "/stop_txn",
            method: "POST",
            contentType: "text/plain",
            data: typeTxn.toString(),
            success: function (data) {
                console.log("Estado de la Transacción...");
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
    }


    function download_logs() {
        setTimeout(function () {
            if (array_global_req.length > 0) {
                download_log_request();
                setTimeout(function () {
                    if (array_global_req.length > 0) {
                        download_log_response();
                    } else {
                        swal({
                            text: "No hay Respuestas en el Log!",
                            icon: "warning",
                            button: {
                                text: "Regresar",
                            },
                        });
                    }
                }, 500);
            } else {
                swal({
                    text: "No hay Peticiones en el Log!",
                    icon: "warning",
                    button: {
                        text: "Regresar",
                    },
                });
            }
        }, 500);
    }

    function download_log_request() {
        var blob_request = new Blob(["Peticiones Log\n", writeArrayLog(array_global_req)], {
            type: "text/plain;charset=utf-8;",
        });
        saveAs(blob_request, "Peticiones Log.txt");
    }

    function download_log_response() {

        var blob_response = new Blob(["Respuestas Log\n", writeArrayLog(array_global_res)], {
            type: "text/plain;charset=utf-8;",
        });
        saveAs(blob_response, "Respuestas Log.txt");
    }

    function see_logs() {
        setTimeout(function () {
            var req_win = window.open("", "Peticiones Log");
            req_win.document.write("<textarea style='width: 100%; height: 100%'>");
            req_win.document.write("********** LOG PETICIONES ***********\n");
            req_win.document.write(writeArrayLog(array_global_req));
            req_win.document.write("</textarea>");

            setTimeout(function () {
                var res_win = window.open("", "Respuestas Log");
                res_win.document.write("<textarea style='width: 100%; height: 100%'>");
                res_win.document.write("********** LOG RESPUESTAS ***********\n");
                res_win.document.write(writeArrayLog(array_global_res));
                res_win.document.write("</textarea>");
            }, 500);
        }, 500);
    }

    function writeArrayLog(arrayLog) {
        let stringLog = "";

        $.each(arrayLog, function (i, txn) {
            //Se añade el titulo y fecha de la TXN

            stringLog += "\n===== " + txn.count + ". " + txn.operacion + " " + txn.date + " =====\n";
            stringLog += JSON.stringify(JSON.parse(txn.json), undefined, 4) + "\n";
        });
        return stringLog;
    }

    function add_txn(array, type_funct) {
        let ta, table;
        switch (type_funct) {
            case 1:
                table = "table-request";
                ta = "log_request";
                $.each(array, function (i, txn) {
                    console.log(txn);
                    append_table(count_req, txn.date, table, txn.operacion);
                    add_textarea(txn.json, ta);
                    add_to_array(count_req, txn.date, txn.json, txn.operacion, array_global_req);
                    count_req++;
                });
                break;
            case 2:
                table = "table-response";
                ta = "log_response";
                $.each(array, function (i, txn) {
                    //console.log(txn);
                    append_table(count_res, txn.date, table, txn.operacion);
                    add_textarea(txn.json, ta);
                    add_to_array(count_res, txn.date, txn.json, txn.operacion, array_global_res);
                    count_res++;
                });
                break;
        }

    }

    /*function init_checkbox(){
                $("#cb_emv").prop("checked", false);
        $("#cb_s71").prop("checked", false);
        $("#cb_s72").prop("checked", false);
    }*/

    function add_to_array(count, date, json, operacion, array_g) {
        let txn_aux = {
            "count": count,
            "date": date,
            "operacion": operacion,
            "json": json
        }
        array_g.push(txn_aux);
    }

    function add_textarea(new_json, textarea) {
        var prety_json = JSON.stringify(JSON.parse(new_json), undefined, 4);
        //var old_value = $("log_request").val();
        //var new_value = old_value + "\n";
        //$("#" + textarea).val(prety_json.replace(/\s\s{2,}}|,|{|}/g, ""));
        $("#" + textarea).val(prety_json);
    }

    function append_table(count, date, table, operacion) {
        $("#" + table + " tbody").prepend("<tr class='tr_txn count-" + count + "' data-count='" + count + "'><td>" + count + ". " + operacion + " - " + date + "</td></tr>");
    }

    function addReqResToTextarea(count_txn) {
        let txn_req = getTxnRequest(count_txn);
        let txn_res = getTxnResponse(count_txn);
        $(".count-" + count_txn).addClass("active-table");
        add_textarea(txn_req.json, "log_request");
        add_textarea(txn_res.json, "log_response");
    }

    function getTxnResponse(count_txn) {
        return array_global_res.find(function (element, index) {
            if (index === count_txn - 1) {
                return element;
            }
        });
    }

    function getTxnRequest(count_txn) {
        return array_global_req.find(function (element, index) {
            if (index === count_txn - 1) {
                return element;
            }
        });
    }

    function findJson(txn, count) {
        return txn.count === count;
    }

    function tablaFormatter(json_string) {
        //Se agregan lineas prety_json.replace(/\s\s{2,}}|,|{|}/g, "")
        let json_table = json_string.replace(/:{/g, "\n==============================\n\t");
        json_table = json_table.replace(/}/g, "\n*******************************\n");

        //return json_string;
        return json_table;
    }

    function convert_to_date(date) {
        return date.slice(0, 4) + "-" + date.slice(4, 6) + "-" + date.slice(6, 8) + " " + date.slice(-6, -4) + ":" + date.slice(-4, -2) + ":" + date.slice(-2);
    }

    function offRefreshLog() {
        refresh_flag = false;
        $("#btn_refresh_log").bootstrapToggle('off');
    }

    function removeActiveClass(table) {
        $(table + " tr").removeClass("active-table");
    }

    function verify_checked_keys(check) {
        $(".checkbox_key").prop("checked", false);
        check.prop("checked", true);
    }

    function interval_request_to_log() {
        setInterval(function () {
            if (refresh_flag) {
                console.log("Refreshing...");
                removeActiveClass("#table-request");
                removeActiveClass("#table-response");
                request_to_log();
                response_to_log();
                //request_to_log_test(1);
                //request_to_log_test(2);
            }
        }, 2000);
    }

    function get_status_txn() {
        $.ajax({
            async: true,
            url: "/get_status_stop_txn",
            method: "GET",
            success: function (data) {
                console.log("Estado de la Transacción...");
                console.log(data);
                $("#btn_stop").prop("checked", (data === 1));
                $("#btn_stop").trigger('change');
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
    }

    function get_status_emv() {
        $.ajax({
            async: true,
            url: "/get_status_emv",
            method: "GET",
            success: function (data) {
                console.log("Estado de EMV...");
                console.log(data);
                if(data === "1" || data === 1) {
                    $("#cb_emv").prop('checked', true);
                    validate_checkbox();
                }
                /*$("#btn_stop").prop("checked", (data === 1));
                $("#btn_stop").trigger('change');*/
            },
            error: function (data) {
                console.log("ERROR");
            }
        });
    }
});

function test() {
    if (document.getElementById('label_enviar_transaccion').textContent == "Enviar Transaccion") {

        document.getElementById('label_log_encendido').innerHTML = 'On';
        document.getElementById('label_log_apagado').innerHTML = 'Off';

        document.getElementById('btn_cambia_idioma').innerHTML = 'Change Lenguage (Spanish)';

        document.getElementById('label_qps_activo').innerHTML = 'QPS On';
        document.getElementById('label_qps_inactivo').innerHTML = 'QPS Off';
        document.getElementById('label_estado_transaccion').innerHTML = 'Stop Transaction';
        document.getElementById('label_icon_enable').innerHTML = 'Enable';
        document.getElementById('label_icon_disable').innerHTML = 'Disable';

        document.getElementById('label_title').innerHTML = 'TotalPos SDK Simulator';
        document.getElementById('label_enviar_transaccion').innerHTML = 'Send Transaction ';
        document.getElementById('label_carga_llaves').innerHTML = 'Load Keys ';
        document.getElementById('label_telecarga_automatica').innerHTML = 'Telecarga Automatic ';
        document.getElementById('label_qps_fijo').innerHTML = 'QPS - Fixed Amount : $300 ';
        document.getElementById('label_actualizar_logs').innerHTML = 'Update Log  ';
        document.getElementById('label_ver_descargar_logs').innerHTML = 'View and Download Logs';
        document.getElementById('label_logTransacciones').innerHTML = 'Transaction Log';
        document.getElementById('label_peticiones').innerHTML = 'Request';
        document.getElementById('label_respuestas').innerHTML = 'Response';
        document.getElementById('label_ver_logs').innerHTML = 'View';
        document.getElementById('label_descargar_logs').innerHTML = 'Download Logs';
        document.getElementById('label_llaves_sugerida').innerHTML = 'Suggested Key Loading';
        document.getElementById('label_llaves_obligatoria').innerHTML = 'Mandatory Key Loading';
        document.getElementById('label_telecarga_requerida').innerHTML = 'Telecarga automatic required';
        document.getElementById('label_00').innerHTML = '00 - Approved';
        document.getElementById('label_00DCC').innerHTML = '00 - Approved DCC';
        document.getElementById('label_01').innerHTML = '01 - Call the issuer';
        document.getElementById('label_03').innerHTML = '03 - Invalid Merchant';
        document.getElementById('label_04').innerHTML = '04 - Pick up card';
        document.getElementById('label_05').innerHTML = '05 - Declined',
            document.getElementById('label_06').innerHTML = '06 - Original transaction not found',
            document.getElementById('label_12').innerHTML = '12 - Invalid Transaction';
        document.getElementById('label_13').innerHTML = '13 - Invalid Amount';
        document.getElementById('label_14').innerHTML = '14 - Invalid card number';
        document.getElementById('label_30').innerHTML = '30 - Format error';


        document.getElementById('label_40').innerHTML = '40 - Request function not supported';
        document.getElementById('label_41').innerHTML = '41 - Lost Card, Pick up';
        document.getElementById('label_43').innerHTML = '43 - Stolen Card, Pick up';
        document.getElementById('label_45').innerHTML = '45 - Promotion not allowed';
        document.getElementById('label_46').innerHTML = '46 - Lower minimum amount';
        document.getElementById('label_47').innerHTML = '47 - Transaction not performed due to exceeding your allowed limit';

        document.getElementById('label_48').innerHTML = '48 - CV2 Required';
        document.getElementById('label_49').innerHTML = '49 - CV2 Invalid';
        document.getElementById('label_50').innerHTML = '50 - You have exceeded the number of rejected transactions';

        document.getElementById('label_51').innerHTML = '51 - Not sufficient funds';
        document.getElementById('label_53').innerHTML = '53 - No savings account';
        document.getElementById('label_54').innerHTML = '54 - Expired Card';
        document.getElementById('label_55').innerHTML = '55 - Incorrect PIN';
        document.getElementById('label_57').innerHTML = '57 - Transaction not Permitted';
        document.getElementById('label_61').innerHTML = '61 - Exceeds withdrawal amount limit';
        document.getElementById('label_62').innerHTML = '62 - Restricted Card';
        document.getElementById('label_65').innerHTML = '65 - Exceeds Withdrawal Frequency Limit';
        document.getElementById('label_69').innerHTML = '69 - Cell Number not Associated with Express Account';
        document.getElementById('label_70').innerHTML = '70 - Error decrypting Track2';
        document.getElementById('label_71').innerHTML = '71 - Must initialize keys';
        document.getElementById('label_72').innerHTML = '72 - Problem initializing Keys';
        document.getElementById('label_73').innerHTML = '73 - CRC error';
        document.getElementById('label_75').innerHTML = '75 - Allowable number of PIN tries exceeded';
        document.getElementById('label_76').innerHTML = '76 - Ineligible account';
        document.getElementById('label_82').innerHTML = '82 - Private (Security Box)';
        document.getElementById('label_83').innerHTML = '83 - Private (no accounts)';
        document.getElementById('label_92').innerHTML = '92 - Verify amount';
        document.getElementById('label_93').innerHTML = '93 - Transaction cannot be complete';
        document.getElementById('label_A3').innerHTML = 'A3 - Balance limit exceeded with deposit';
        document.getElementById('label_A4').innerHTML = 'A4 - With this deposit you exceed the limit allowed for this product per month';


        document.getElementById('label_B1').innerHTML = 'B1 - Campaign data transaction';
        document.getElementById('label_B2').innerHTML = 'B2 - Service not available';
        document.getElementById('label_C1').innerHTML = 'C1 - Undefined product';
        document.getElementById('label_C2').innerHTML = 'C2 - Sold product';
        document.getElementById('label_C3').innerHTML = 'C3 - Invalid product for sale';

        document.getElementById('label_C4').innerHTML = 'C4 - Promotion Finished';
        document.getElementById('label_C5').innerHTML = 'C5 - Without sales authorization';
        document.getElementById('label_C6').innerHTML = 'C6 - Unauthorized sale of product';
        document.getElementById('label_C7').innerHTML = 'C7 - Sale not allowed by type of transaction';

        document.getElementById('label_C8').innerHTML = 'C8 - Undefined terms';
        document.getElementById('label_C9').innerHTML = 'C9 - Maximum number of sales';

        document.getElementById('label_C1').innerHTML = 'CA - Undefined product';

        document.getElementById('label_CB').innerHTML = 'CB - Product cannot be returned';
        document.getElementById('label_D11').innerHTML = 'D1 - Status 1 - DCC Conversion Transaction';
        document.getElementById('label_D12').innerHTML = 'D1 - Status 2 - DCC Conversion Transaction';
        document.getElementById('label_D2').innerHTML = 'D2 - Check-Out error - retry';
        document.getElementById('label_D3').innerHTML = 'D3 - DCC conversion amount error';
        document.getElementById('label_R1').innerHTML = 'R1 - Registration terminal';
        document.getElementById('ModalTitleEMV').innerHTML = 'Choose some option...';


    } else {

        document.getElementById('btn_cambia_idioma').innerHTML = 'Cambia Idioma (English)';

        document.getElementById('label_log_encendido').innerHTML = 'Encendido';
        document.getElementById('label_log_apagado').innerHTML = 'Apagado';

        document.getElementById('label_qps_activo').innerHTML = 'QPS Activo';
        document.getElementById('label_qps_inactivo').innerHTML = 'QPS Inactivo';
        document.getElementById('label_estado_transaccion').innerHTML = 'Detener Transacción';
        document.getElementById('label_icon_enable').innerHTML = 'Activo';
        document.getElementById('label_icon_disable').innerHTML = 'Inactivo';

        document.getElementById('label_title').innerHTML = 'Simulador SDK TotalPos';
        document.getElementById('label_enviar_transaccion').innerHTML = 'Enviar Transaccion';
        document.getElementById('label_carga_llaves').innerHTML = 'Carga de Llaves';
        document.getElementById('label_telecarga_automatica').innerHTML = 'Telecarga Automatica';
        document.getElementById('label_qps_fijo').innerHTML = 'QPS - Monto Fijo : $300 ';
        document.getElementById('label_actualizar_logs').innerHTML = 'Actualizar Log';
        document.getElementById('label_ver_descargar_logs').innerHTML = 'Ver y Descargar Logs';
        document.getElementById('label_logTransacciones').innerHTML = 'Log de Transacciones';
        document.getElementById('label_peticiones').innerHTML = 'Peticiones';
        document.getElementById('label_respuestas').innerHTML = 'Respuestas';
        document.getElementById('label_ver_logs').innerHTML = 'Ver';
        document.getElementById('label_descargar_logs').innerHTML = 'Descargar Logs';
        document.getElementById('label_llaves_sugerida').innerHTML = 'Carga de Llaves Sugerida';
        document.getElementById('label_llaves_obligatoria').innerHTML = 'Carga de Llaves Obligatoria';
        document.getElementById('label_telecarga_requerida').innerHTML = 'Telecarga Automatica Requerida';
        document.getElementById('label_00').innerHTML = '00 - Aprobada';
        document.getElementById('label_00DCC').innerHTML = '00 - Aprobada DCC';
        document.getElementById('label_01').innerHTML = '01 - Llame al emisor';
        document.getElementById('label_03').innerHTML = '03 - Negocio Inválido';
        document.getElementById('label_04').innerHTML = '04 - Recoger Tarjeta';
        document.getElementById('label_05').innerHTML = '05 - Declinada',
            document.getElementById('label_06').innerHTML = '06 - Transaccion Original no encontrada',
            document.getElementById('label_12').innerHTML = '12 - Transacción inválida (Fallback)';
        document.getElementById('label_13').innerHTML = '13 - Monto Inválido';
        document.getElementById('label_14').innerHTML = '14 - Tarjeta Inválida';
        document.getElementById('label_30').innerHTML = '30 - Error de formato';


        document.getElementById('label_40').innerHTML = '40 - Función no soportada';
        document.getElementById('label_41').innerHTML = '41 - Recoger Tarjeta';
        document.getElementById('label_43').innerHTML = '43 - Recoger Tarjeta';
        document.getElementById('label_45').innerHTML = '45 - Promoción no permitida';
        document.getElementById('label_46').innerHTML = '46 - Monto inferior mín promo';
        document.getElementById('label_47').innerHTML = '47 - Transacción no realizada por haber excedido su límite permitido.';

        document.getElementById('label_48').innerHTML = '48 - CV2 Requerido';
        document.getElementById('label_49').innerHTML = '49 - CV2 Inválido';
        document.getElementById('label_50').innerHTML = '50 - Ha superado el número de transacciones rechazadas';

        document.getElementById('label_51').innerHTML = '51 - Saldo insuficiente';
        document.getElementById('label_53').innerHTML = '53 - Cuenta inexistente';
        document.getElementById('label_54').innerHTML = '54 - Tarjeta Expirada';
        document.getElementById('label_55').innerHTML = '55 - NIP incorrecto';
        document.getElementById('label_57').innerHTML = '57 - Comercio No Marcado';
        document.getElementById('label_61').innerHTML = '61 - Excede límite de monto';
        document.getElementById('label_62').innerHTML = '62 - Bin De Tarjeta No Permitido';
        document.getElementById('label_65').innerHTML = '65 - Intentos De Retiros Excedido';
        document.getElementById('label_69').innerHTML = '69 - Número Celular no Asociado a Cuenta Express';
        document.getElementById('label_70').innerHTML = '70 - Error descifrando Track2';
        document.getElementById('label_71').innerHTML = '71 - Debe inicializar llaves';
        document.getElementById('label_72').innerHTML = '72 - Problema inicializando Llaves';
        document.getElementById('label_73').innerHTML = '73 - Error en CRC';
        document.getElementById('label_75').innerHTML = '75 - Número de intentos de NIP excedidos';
        document.getElementById('label_76').innerHTML = '76 - Cuenta bloqueada';
        document.getElementById('label_82').innerHTML = '82 - CVV/CVV2 incorrecto';
        document.getElementById('label_83').innerHTML = '83 - Rechazada';
        document.getElementById('label_92').innerHTML = '92 - Verifique el Importe';
        document.getElementById('label_93').innerHTML = '93 - Operación no disponible';
        document.getElementById('label_A3').innerHTML = 'A3 - Límite de saldo superado con depósito';
        document.getElementById('label_A4').innerHTML = 'A4 - Con este depósito excede el límite permitido para este producto por mes';


        document.getElementById('label_B1').innerHTML = 'B1 - Transaccion Con Datos de Campaña';
        document.getElementById('label_B2').innerHTML = 'B2 - Servicio No Disponible';

        document.getElementById('label_C1').innerHTML = 'C1 - Producto No Definido';
        document.getElementById('label_C2').innerHTML = 'C2 - Producto Vendido';
        document.getElementById('label_C3').innerHTML = 'C3 - Producto Invalido para Venta';
        document.getElementById('label_C4').innerHTML = 'C4 - Promocion Finalizada';
        document.getElementById('label_C5').innerHTML = 'C5 - Sin Autorizacion de Venta';
        document.getElementById('label_C6').innerHTML = 'C6 - Venta No Permitida de producto';
        document.getElementById('label_C7').innerHTML = 'C7 - Venta No Permitida Por Tipo deTransaccion';
        document.getElementById('label_C8').innerHTML = 'C8 - Plazos No Definidos';
        document.getElementById('label_C9').innerHTML = 'C9 - Número máximo de venta';
        document.getElementById('label_C1').innerHTML = 'CA - Monto de transacción invalido';
        document.getElementById('label_CB').innerHTML = 'CB - Producto no puede ser devuelto';

        document.getElementById('label_D11').innerHTML = 'D1 - Estatus 1 - Transaccion de conversion a DCC';
        document.getElementById('label_D12').innerHTML = 'D1 - Estatus 2 - Transaccion de conversion a DCC';
        document.getElementById('label_D2').innerHTML = 'D2 - Error en el Check-Out - Reintente';
        document.getElementById('label_D3').innerHTML = 'D3 - Error en el monto de conversi&oacute;n de DCC';
        document.getElementById('label_R1').innerHTML = 'R1 - Registre Terminal';
        document.getElementById('ModalTitleEMV').innerHTML = 'Selecciona alguna opción...';


    }
}

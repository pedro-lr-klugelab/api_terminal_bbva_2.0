<%@ page language="java" contentType="text/html; charset=ISO-8859-1"
         pageEncoding="ISO-8859-1" %>
<%@ taglib uri="http://java.sun.com/jsp/jstl/core" prefix="c" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <meta http-equiv="Pragma" content="no-cache">
    <meta http-equiv="Cache-Control" content="no-cache">
    <meta http-equiv="Expires" content="0">

    <title>Simulador</title>
    <link rel=icon href=./img/favicon.ico sizes="16x16" type="image/ico">
    <%-- CSS --%>
    <link rel='stylesheet' type='text/css' href="./styles/bootstrap.min.css">
    <link rel='stylesheet' type='text/css' href="./styles/font-awesome.min.css">
    <link rel='stylesheet' type='text/css' href="./styles/main.css">
    <!-- <link rel='stylesheet' type='text/css' href="./styles/bootstrap2-toggle.min.css">-->
    <link rel="stylesheet" type='text/css' href="./styles/bootstrap4-toggle.min.css">
    <%-- JS --%>
    <script type="text/javascript" src="./js/jquery-3.3.1.min.js"></script>
    <script type="text/javascript" src="./js/popper.min.js"></script>
    <script type="text/javascript" src="./js/bootstrap.min.js"></script>
    <script type="text/javascript" src="./js/FileSaver.min.js"></script>
    <script type="text/javascript" src="./js/mdtimepicker.min.js"></script>
    <script type="text/javascript" src="./js/moment.min.js"></script>
    <script type="text/javascript" src="./js/sweetalert.min.js"></script>
    <script type="text/javascript" src="./js/bootstrap4-toggle.min.js"></script>
    <script type="text/javascript" src="./js/endpoints.js"></script>
</head>
<body>
<div id="div_alert">
    <div class="toast" id="toast_txn" role="alert" data-delay="2000">
        <div class="toast-header">
            <img src="./img/logo_eg.png" class="rounded mr-2 img-toast">
            <strong class="mr-auto">Simulador</strong>
            <button type="button" class="ml-2 mb-1 close" data-dismiss="toast" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
        <div class="toast-body"><label id="toast_body"></label></div>
    </div>
</div>
<div class="row d-flex flex-row">
    <img src="./img/logo_eglobal.png" class="logo-header mx-3 mt-2">
    <h1 id='label_title' class="mt-2">Simulador SDK TotalPos</h1>
</div>

<div class="row col-md-12">
    <div class="col-md-12 ml--10 mt-2 bg-primary text-white">
        <p id="versionP" class="text-right"></p>
    </div>
</div>

<div class="row col-md-12">
    <div class="col-md-3 ml--30 mt-5">
        <ul class="nav flex-column card shadow p-3">
            <li class="nav-item w-100">
                <div class="col-md-12">
                    <button id='btn_cambia_idioma' class="btn btn-info btn-width" onclick="test();"> Cambiar Idioma
                        (English)
                    </button>
                    <hr>

                    <span id='label_enviar_transaccion'>Enviar Transaccion</span>
                    <div class="row">
                        <div class="col-md-1"></div>
                        <div class="col-md-9">
                            <div class="input-group">
                                <select class="form-control" id="select_type_txn"
                                        name="select_type_txn">
                                    <option value="00" selected id='label_00'>00 - Aprobada</option>
                                    <option value="DCC00" id='label_00DCC'>00 - Aprobada DCC</option>
                                    <option value="01" id='label_01'>01 - Llave al emisor</option>
                                    <option value="03" id='label_03'>03 - Negocio Inv&aacute;lido</option>
                                    <option value="04" id='label_04'>04 - Recoger Tarjeta</option>
                                    <option value="05" id='label_05'>05 - Declinada</option>
                                    <option value="06" id='label_06'>06 - Transacci&oacute;n original no encontrada
                                    <option value="12" id='label_12'>12 - Transacci&oacute;n Inv&aacute;lida
                                        (Fallback)
                                    </option>
                                    <option value="13" id='label_13'>13 - Monto Inv&aacute;lido</option>
                                    <option value="14" id='label_14'>14 - Tarjeta Inv&aacute;lida</option>
                                    <option value="30" id='label_30'>30 - Error De Formato</option>
                                    <option value="40" id='label_40'>40 - Funci&oacute;n No Soportada</option>
                                    <option value="41" id='label_41'>41 - Recoger Tarjeta</option>
                                    <option value="43" id='label_43'>43 - Recoger Tarjeta</option>
                                    <option value="45" id='label_45'>45 - Promoci&oacute;n No Permitida</option>
                                    <option value="46" id='label_46'>46 - Monto Inferior M&iacute;n Promo</option>
                                    <option value="47" id='label_47'>47 - Transacci&oacute;n no realizada
                                        por haber excedido su l&iacute;mite permitido. Acuda a su
                                        sucursal bancaria
                                    </option>
                                    <option value="48" id='label_48'>48 - CV2 Requerido</option>
                                    <option value="49" id='label_49'>49 - CV2 Inv&aacute;lido</option>
                                    <option value="50" id='label_50'>50 - Ha superado el n&uacute;mero de
                                        transacciones rechazadas
                                    </option>
                                    <option value="51" id='label_51'>51 - Saldo Insuficiente</option>
                                    <option value="53" id='label_53'>53 - Cuenta Inexistente</option>
                                    <option value="54" id='label_54'>54 - Tarjeta Expirada</option>
                                    <option value="55" id='label_55'>55 - NIP Incorrecto</option>
                                    <option value="57" id='label_57'>57 - Comercio No Marcado / Marca de
                                        Cash Back o Advance No Permitida
                                    </option>
                                    <option value="61" id='label_61'>61 - Excede L&iacute;mite de Monto</option>
                                    <option value="62" id='label_62'>62 - Bin de Tarjeta no Permitido</option>
                                    <option value="65" id='label_65'>65 - Intentos de Retiros Excedido</option>
                                    <option value="69" id='label_69'>69 - N&uacute;mero Celular No
                                        Asociado a Cuenta Express
                                    </option>
                                    <option value="70" id='label_70'>70 - Error Descifrando Track2</option>
                                    <option value="71" id='label_71'>71 - Debe Inicializar Llaves</option>
                                    <option value="72" id='label_72'>72 - Problema Inicializando Llaves</option>
                                    <option value="73" id='label_73'>73 - Error en CRC</option>
                                    <option value="75" id='label_75'>75 - N&uacute;mero de Intentos de NIP
                                        Excedidos
                                    </option>
                                    <option value="76" id='label_76'>76 - Cuenta Bloqueada</option>
                                    <option value="82" id='label_82'>82 - CVV / CVV2 Incorrecto</option>
                                    <option value="83" id='label_83'>83 - Rechazada</option>
                                    <option value="92" id='label_92'>92 - Verificar Importe</option>
                                    <option value="93" id='label_93'>93 - Operaci&oacute;n No Disponible</option>
                                    <option value="A3" id='label_A3'>A3 - Límite de Saldo Superado Con
                                        Dep&oacute;sito
                                    </option>
                                    <option value="A4" id='label_A4'>A4 - Con Este Dep&oacute;sito Excede
                                        el L&iacute;mite Permitido Para Este Producto por Mes
                                    </option>
                                    <option value="B1" id='label_B1'>B1 - Transacci&oacute;n Con Datos de
                                        Campa&ntilde;a
                                    </option>
                                    <option value="B2" id='label_B2'>B2 - Servicio No Disponible.
                                        Promociones Especiales
                                    </option>
                                    <option value="C1" id='label_C1'>C1 - Producto No Definido</option>
                                    <option value="C2" id='label_C2'>C2 - Producto Vendido</option>
                                    <option value="C3" id='label_C3'>C3 - Producto Inv&aacute;lido Para
                                        Venta
                                    </option>
                                    <option value="C4" id='label_C4'>C4 - Promoci&oacute;n Finalizada</option>
                                    <option value="C5" id='label_C5'>C5 - Sin Autorizaci&oacute;n de Venta</option>
                                    <option value="C6" id='label_C6'>C6 - Venta No Permitida de producto</option>
                                    <option value="C7" id='label_C7'>C7 - Venta No Permitida Por Tipo de
                                        Transacci&oacute;n
                                    </option>
                                    <option value="C8" id='label_C8'>C8 - Plazos No Definidos</option>
                                    <option value="C9" id='label_C9'>C9 - N&uacute;mero M&aacute;ximo de
                                        Venta
                                    </option>
                                    <option value="CA" id='label_C1'>CA - Monto de Transacci&oacute;n
                                        Inv&aacute;lido
                                    </option>
                                    <option value="CB" id='label_CB'>CB - Producto No Puede Ser Devuelto</option>
                                    <option value="D11" id='label_D11'>D1 Estatus 1 - Transacci&oacute;n de conversi&oacute;n
                                        a DCC
                                    </option>
                                    <option value="D12" id='label_D12'>D1 Estatus 2 - Transacci&oacute;n de conversi&oacute;n
                                        a DCC
                                    </option>
                                    <option value="D2" id='label_D2'>D2 - Error en el Check-Out - Reintente</option>
                                    <option value="D3" id='label_D3'>D3 - Error en el monto de conversi&oacute;n de
                                        DCC
                                    </option>
                                    <option value="R1" id='label_R1'>R1 - Registre Terminal</option>

                                </select>
                            </div>
                        </div>
                    </div>
                    <hr>
                </div>
            </li>
            <li class="nav-item w-100 modenaDCC" hidden="true">
                <div class="col-md-12">
                    <label>DCC - Divisas participantes</label>
                    <div class="row">
                        <div class="col-md-1"></div>
                        <div class="col-md-9">
                            <div class="input-group">
                                <select class="form-control" id="select_dcc_moneda"
                                        name="select_type_txn">
                                    <option value="USD" selected>USD - D&oacute;lar Americano</option>
                                    <option value="CAD">CAD - D&oacute;lar Canadiense</option>
                                    <option value="EUR">EUR - Moneda de la Uni&oacute;n Europea</option>
                                    <option value="SEK">SEK - Corona Sueca</option>
                                    <option value="CHF">CHF - Franco Suizo</option>
                                    <option value="JPY">JPY - Yen Japon&eacute;s</option>
                                    <option value="GBP">GBP - Libra Esterlina Reino Unido</option>
                                </select>
                            </div>
                        </div>
                    </div>
                    <hr>
                </div>
            </li>
            <li class="nav-item w-100">
                <div class="col-md-12">
                    <span id='label_estado_transaccion'>Detener Transacción</span>

                    <div class="row">
                        <div class="col-md-1"></div>
                        <div class="col-md-9">
                            <input class="btn-width" id="btn_stop" type="checkbox"
                                   data-toggle="toggle"
                                   data-on="<i class='fa fa-play-circle-o'></i> <span id='label_icon_enable'>Activo </span>"
                                   data-off="<i class='fa fa-stop-circle-o'></i> <span id='label_icon_disable'>Inactivo </span>"
                                   data-onstyle="success" data-offstyle="danger">
                        </div>
                    </div>
                    <hr>
                </div>
            </li>

            <li class="nav-item w-100">
                <div class="col-md-12">
                    <span id='label_script'>EMV</span>
                    <i class="fa fa-file-code-o"></i>
                    <div class="row">
                        <div class="col-md-1"></div>
                        <div class="col-md-9">
                            <button type="button" class=" btn-width btn btn-primary" data-toggle="modal"
                                    data-target="#modal_EMV">
                                EMV
                            </button>
                        </div>
                    </div>
                    <hr>
                </div>
            </li>


            <!-- <li class="nav-item w-100">
            <div class="col-md-12">
                <label>Enviar MO/TO</label>
                <div class="row">
                    <div class="col-md-1"></div>
                    <div class="col-md-9">
                        <input class="btn-width" id="btn_moto" type="checkbox"
                      data-toggle="toggle"
                      data-on="<i class='fa fa-play-circle-o'></i> MO/TO"
                      data-off="<i class='fa fa-stop-circle-o'></i> MO/TO"
                      data-onstyle="info" data-offstyle="outline-info">
                    </div>
                </div>
              <hr>
            </div>
            </li>-->
            <li class="nav-item w-100">
                <div class="col-md-12">

                    <span id='label_carga_llaves'>Carga de Llaves  </span><i class='fa fa-key'></i>
                    <div class="mt-2 ml-4">
                        <label class="container">
                            <input type="checkbox" class="checkbox_key" data-value="1">
                            <span class="checkmark-box"></span>
                            <span id='label_llaves_sugerida'>Carga de Llaves Sugerida</span>

                        </label>
                        <label class="container">
                            <input type="checkbox" class="checkbox_key" data-value="3">
                            <span class="checkmark-box"></span>
                            <span id='label_llaves_obligatoria'>Carga de Llaves Obligatoria</span>
                        </label>
                    </div>
                    <hr>
                    <label id='label_telecarga_automatica'>Telecarga Autom&aacute;tica </label><i class='fa fa-key'></i>
                    <div class="mt-2 ml-4">
                        <label class="container">
                            <input type="checkbox" class="checkbox_key" data-value="2">
                            <span class="checkmark-box"></span>
                            <span id='label_telecarga_requerida'>Telecarga Automatica Requerida</span>
                        </label>
                    </div>
                    <hr>
                </div>
            </li>
            <li class="nav-item w-100">
                <div class="col-md-12">
                    <%--<label id='label_qps_fijo'>QPS - Monto fijo : $300</label>--%>
                    <div class="form-group row align-items-center">
                        <label id='label_qps_fijo'>QPS - Monto fijo:</label>
                        <div class="col">
                            <input type="number" class="form-control" min="1" id="qps_value" value="300">
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-1"></div>
                        <div class="col-md-9">
                            <input class="btn-width" id="btn_qps" type="checkbox"
                                   data-toggle="toggle"
                                   data-on="<i class='fa fa-play-circle-o'></i> <span id='label_qps_activo'>QPS Activo</span>"
                                   data-off="<i class='fa fa-stop-circle-o'></i><span id='label_qps_inactivo'>QPS Inactivo</span>"
                                   data-onstyle="info" data-offstyle="outline-info">
                        </div>
                    </div>
                    <hr>
                </div>
            </li>
            <li class="nav-item w-100">
                <div class="col-md-12">
                    <label id='label_actualizar_logs'>Actualizar Log </label><i class="fa fa-refresh"></i>
                    <div class="row">
                        <div class="col-md-1"></div>
                        <div class="col-md-9">
                            <input class="btn-width" id="btn_refresh_log" type="checkbox" checked
                                   data-toggle="toggle"
                                   data-on="<span id='label_log_encendido'>Encendido</span>"
                                   data-off="<span id='label_log_apagado'>Apagado</span>"
                                   data-onstyle="success" data-offstyle="danger">
                        </div>
                    </div>
                    <hr>
                </div>
            </li>
            <li class="nav-item w-100">
                <div class="col-md-12">
                    <label id='label_ver_descargar_logs'>Ver y Descargar Logs </label><i class="fa fa-files-o"></i>
                    <div class="row">
                        <div class="col-md-6">
                            <button class="btn btn-primary btn-width" id="btn_see_logs" type="button"><label
                                    id='label_ver_logs'>Ver Logs </label> <i class="fa fa-eye"></i></button>
                        </div>
                        <div class="col-md-6">
                            <button class="btn btn-primary btn-width" id="btn_download_logs" type="button"><label
                                    id='label_descargar_logs'>Descargar Logs </label> <i class="fa fa-download"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </li>
        </ul>
    </div>
    <div class="col-md-9 mt--05">
        <div class="row">
            <div class="col-md-12">
                <br><br><br>
                <ul class="nav nav-tabs" id="log_tab" role="tablist">
                    <li class="nav-item"><a class="nav-link active"
                                            id="request-tab" data-toggle="tab" href="#request" role="tab"
                                            aria-controls="request" aria-selected="true"> <label id='label_peticiones'>Peticiones</label>
                    </a></li>
                    <li class="nav-item"><a class="nav-link" id="response-tab"
                                            data-toggle="tab" href="#response" role="tab"
                                            aria-controls="response" aria-selected="false"><label id='label_respuestas'>Respuestas</label></a>
                    </li>
                </ul>
                <div class="tab-content card shadow">
                    <div class="tab-pane fade show active" id="request"
                         role="tabpanel" aria-labelledby="request-tab">
                        <div class="d-flex justify-content-start">
                            <table id="table-request"
                                   class="table table-striped table-bordered table-hover table-log">
                                <tbody>
                                </tbody>
                            </table>
                            <textarea id="log_request" class="log-textarea"></textarea>
                        </div>
                    </div>
                    <div class="tab-pane fade" id="response" role="tabpanel"
                         aria-labelledby="response-tab">
                        <div class="d-flex justify-content-start">
                            <table id="table-response"
                                   class="table table-striped table-bordered table-hover table-log">
                                <tbody>
                                </tbody>
                            </table>
                            <textarea id="log_response" class="log-textarea"></textarea>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Modal -->
<div class="modal fade" id="modal_EMV" tabindex="-1" role="dialog"
     aria-labelledby="ModalTitleEMV" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered" role="document">
        <div class="modal-content">
            <div class="modal-header bg-primary text-white">
                <h5 class="modal-title" id="ModalTitleEMV">Selecciona alguna opción...</h5>
                <button type="button" class="close" data-dismiss="modal"
                        aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <div class="row">
                    <div class="col-md-4">
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox" value="" id="cb_emv">
                            <label class="form-check-label" for="cb_emv">
                                Tag 91
                            </label>
                        </div>
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox" value="" id="cb_s71">
                            <label class="form-check-label" for="cb_s71">
                                Script 71
                            </label>
                        </div>
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox" value="" id="cb_s72">
                            <label class="form-check-label" for="cb_s72">
                                Script 72
                            </label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-primary" id="btn_send_script">Enviar</button>
                <button type="button" class="btn btn-primary" id="btn_cancel_script">Cancelar</button>
            </div>
        </div>
    </div>
</div>
</body>
</html>

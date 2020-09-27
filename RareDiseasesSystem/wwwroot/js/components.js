(function () {
    Vue.component("v-user-menu", {
        template: "#v-user-menu",
        props: ['loginUser'],
        methods: {
            menuClick: function (command) {
                if (command === "logout") {
                    this.logout();
                }            
            },
            logout: function () {
                if (confirm("您确定要注销么？")) {
                    var returnUrl = window.location.href;
                    window.location.href = "/Login/Index?Logout=true&ReturnUrl=" + encodeURI(returnUrl);
                }
            }
        }
    });
})();

(function () {
    Vue.component("v-disease-search", {
        data: function () {
            return {
                search: "帕金森",
                deseaseSearchedList: [],
                total: 0,
                pageIndex: 1,
                loading: false,
                iframeSrc: " ",
                pageSize:5
            };
        },
        template: "#v-disease-search",
        methods: {
            onSearch: function () {
                this.pageIndex = 1;
                this.searchDiseaseList();
            },
            searchDiseaseList: function () {
                var param = {};
                param.search = this.search;
                param.pageIndex = this.pageIndex;
                param.pageSize = 5;
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/RareDisease/SearchList",
                    type: "GET",
                    data: param,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.deseaseSearchedList = data.data;
                            that.total = data.total;
                        }
                        else {
                            console.log(data.msg);
                        }
                        that.loading = false;
                    }
                });
            },
            open121HpoDetail: function (subgroup) {
                document.getElementById("iframeDiv").innerHTML = "";
                var iframeSrc = "../html/121.html" + subgroup.anchor;


                //在document中创建iframe
                var iframe = document.createElement("iframe");

                //设置iframe的样式
                iframe.style.width = "100%";
                iframe.style.height = "calc(100vh - 100px)";
                iframe.style.margin = "0";
                iframe.style.padding = "0";
                iframe.style.border = "1 solid  rgb(243,247,254)";

                //绑定iframe的onload事件,处理事件的兼容问题
                if (
                    onload &&
                    Object.prototype.toString.call(onload) === "[object Function]"
                ) {
                    if (iframe.attachEvent) {
                        iframe.attachEvent("onload", onload);
                    } else if (iframe.addEventListener) {
                        iframe.addEventListener("load", onload);

                    } else {
                        iframe.onload = onload;
                    }
                }

                iframe.src = iframeSrc;
                //把iframe载入到dom以下
                document.getElementById("iframeDiv").appendChild(iframe);
                return iframe;
            }
        }
    });

})();

(function () {
    Vue.component("v-disease-cdss", {
        data: function () {
            return {
                importEMRDlg: false,
                patientCardNo: "",
                patientEmpiId:"",
                patientOverview: [],
                patientVisitList: [],
                patientVisitListCopy: [],
                patientEMRDetail: "",
                patientVisitListIndex: 1,
                patientVisitListTotal: 0,

                patientHPOList: [],
                searchHPOText: "",
                searchHPODlg: false,
                searchedHPOList: [],
                searchedHPOListCopy: [],
                searchedHPOListTotal: 0,
                searchedHPOListIndex: 1,
                multipleSearchedHPOSelection: [],

                normalDiseaseList: [],
                rareDiseaseList: [],

                nlpEngine: "engine1",
                nlpEngineOptions: [
                    {
                        value: 'engine1',
                        label: 'String Search'
                    },
                    {
                        value: 'engine2',
                        label: 'Spacy'
                    }
                ],
                certainOptions: [{
                    value: '阳性',
                    label: '阳性'
                }, {
                    value: '隐性',
                        label: '隐性'
                }],
                rareAnalyzeEngine: "engine1",
                rareAnalyzeEngineOptions: [
                    {
                        value: 'engine1',
                        label: '相似度1'
                    },
                    {
                        value: 'engine2',
                        label: '相似度2'
                    }
                ],
                rareDataBaseEngine: "eRAM",
                rareDataBaseEngineOptions: [
                    {
                        value: 'eRAM',
                        label: 'eRAM'
                    },
                    {
                        value: 'OMIM',
                        label: 'OMIM'
                    },
                    {
                        value: 'ORPHANET',
                        label: 'ORPHANET'
                    },
                    {
                        value: 'DECIPHER',
                        label: 'DECIPHER'
                    },
                    {
                        value: '整合库',
                        label: '整合库'
                    }
                ]
                
            };
        },
        template: "#v-disease-cdss",    
        methods: {
            onPopImportEMRDlg: function () {
                this.importEMRDlg = true;
            },
            onSearchPatient: function () {
                this.patientVisitListIndex = 1;
                this.requestPatientEMR();
                this.patientEMRDetail = "";
                this.patientEmpiId = "";
            },
            requestPatientEMR: function () {
                var para = {};
                para = {
                    patientCardNo: this.patientCardNo
                };
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Home/SearchPatientData",
                    type: "Get",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.patientVisitListTotal = data.total;
                            that.patientVisitListCopy = data.patientVisitList; 
                            that.patientOverview = data.patientOverview;
                            if (data.patientOverview.length > 0) {
                                that.patientEmpiId = data.patientOverview[0].iEMPINumber;
                            }
                            that.patientLocalPaging();
                        }
                        else {
                            console.log(data);
                        }
                        that.loading = false;
                    }
                });
            },
            patientLocalPaging: function () {
                this.patientVisitList = this.localPaging(this.patientVisitListIndex, this.patientVisitListCopy);
            },

            onImportPatientEMRText: function () {
                var para = {};
                para = {
                    patientEmpiId: this.patientEmpiId                 
                };                
                var that = this;
                $.ajax({
                    url: "/Home/ConvertPatientEMRtoText",
                    type: "Get",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.patientEMRDetail = data.data;
                        }
                        else {
                            console.log(data);
                        }
                    }
                });
                this.importEMRDlg = false;
            },
            onClearPatientEMR: function () {
                this.patientEMRDetail = "";
            },

            onAnalyzePatientEMR: function () {
                if (this.patientEMRDetail === undefined || this.patientEMRDetail === "") {
                    alert('电子病历不能为空！');
                    return false;
                }
                var para = {};
                if (this.patientEmpiId !== "") {
                    para = {
                        patientEmpiId: this.patientEmpiId,
                        nlpEngine: this.nlpEngine
                    };
                }
                else {
                    para = {
                        patientEMRDetail: this.patientEMRDetail,
                        nlpEngine: this.nlpEngine
                    };
                }             
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Home/AnalyzePatientEMRRetreiveHPO",
                    type: "POST",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.patientHPOList = data.data;
                        }
                        else {
                            console.log(data);
                        }
                        that.loading = false;
                    }
                });
            },
            onDeletePatientHPOList: function (subgroup) {
                for (var i = this.patientHPOList.length - 1; i >= 0; i--) {
                    if (this.patientHPOList[i].name === subgroup.name) {
                        this.patientHPOList.splice(i, 1);
                    }
                }
            },
            onShowHpoMatchedText: function (subgroup) {
                var text = $('#txt_patientEMR').val();
                var textBeforePosition = text.substr(0, subgroup.startIndex);
                $('#txt_patientEMR').blur();
                $('#txt_patientEMR').val(textBeforePosition);
                $('#txt_patientEMR').focus();
                $('#txt_patientEMR').val(text);
                $('#txt_patientEMR').selectRange(subgroup.startIndex, subgroup.endIndex); 

            },
            onSearchHPODlg: function () {
                this.searchHPODlg = true;
            },
            onSearchHPOList: function () {
                this.searchedHPOListIndex = 1;
                var para = {};
                para = {
                    searchHPOText: this.searchHPOText
                };
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Home/SearchHPOList",
                    type: "Get",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.searchedHPOListTotal = data.total;
                            that.searchedHPOListCopy = data.data;
                            that.hpoLocalPaging();
                        }
                        else {
                            console.log(data);
                        }
                        that.loading = false;
                    }
                });
            },
            hpoLocalPaging: function () {
                this.searchedHPOList = this.localPaging(this.searchedHPOListIndex, this.searchedHPOListCopy);
            },
            localPaging: function (current,list) {
                var size = 10;
                var tablePush = [];
                for (var i = 0; i < list.length; i++) {
                    if (size * (current - 1) <= i && i <= size * current - 1) {
                        tablePush.push(list[i]);
                    }
                };
                //subList.forEach((item, index) => {
                //    if (size * (current - 1) <= index && index <= size * current - 1) {
                //        tablePush.push(item);
                //    }
                //});
                return tablePush;
            },

            onSearchedHPOSelectionChange: function (val) {
                this.multipleSearchedHPOSelection = val;
            },
            onSearchedHPOSelectionIn: function () {
                var names = this.multipleSearchedHPOSelection.map(function (item) {
                    return item.name;
                });

                if (names.length === 0) {
                    alert("请选择相应的HPO 表征");
                    return;
                }
                else {
                    var that = this;
                    this.multipleSearchedHPOSelection.map(function (item) {
                        var exist = false;
                        for (var i = 0; i < that.patientHPOList.length; i++) {
                            if (that.patientHPOList[i].name === item.name) {
                                exist = true;
                                break;
                            }     
                        }
                        if (!exist) {
                            that.patientHPOList.push(item);
                        }
                        
                    });
                    this.searchHPODlg = false;
                        
                }
            },
            onSubmitHPODataForAnalyze: function () {
                if (this.rareAnalyzeEngine === undefined || this.rareAnalyzeEngine === "") {
                    alert('请选择罕见病分析引擎！');
                    return false;
                }
                if (this.rareDataBaseEngine === undefined || this.rareDataBaseEngine === "") {
                    alert('请选择罕见病数据库！');
                    return false;
                }
                var para = {};
                para = {
                    hpoList: this.patientHPOList,
                    rareAnalyzeEngine: this.rareAnalyzeEngine,
                    rareDataBaseEngine:this.rareDataBaseEngine
                };
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Home/SubmitHPODataForAnalyze",
                    type: "POST",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.normalDiseaseList = data.normalDiseaseList;
                            that.rareDiseaseList = data.rareDiseaseList;
                        }
                        else {
                            console.log(data);
                        }
                        that.loading = false;
                    }
                });
            }
        }
    });
})();



(function () {
    Vue.component("v-operation-log", {
        data: function () {
            return {
                logList: [],
                total: 0,
                pageIndex: 1,
                loading: false,
                logDateRange:""
            };
        },
        template: "#v-operation-log",
        methods: {

            onSearch: function () {
                this.pageIndex = 1;
                this.searchOperationLogList();
            },
            searchOperationLogList: function () {
                var param = {};
                param.pageIndex = this.pageIndex;
                param.pageSize = 10;
                param.logDateRange = this.logDateRange;
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Log/SearchLogList",
                    type: "GET",
                    data: param,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.logList = data.data;
                            that.total = data.total;
                        }
                        else {
                            console.log(data.msg);
                        }
                        that.loading = false;
                    }
                });
            }
        },
        mounted: function () {
            this.onSearch();
        }
    });

})();
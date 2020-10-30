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
                number: "", //number 可以是empiid 或者身份证号

                numberType: "empi",
                numberTypeOptions: [
                    {
                        value: 'empi',
                        label: 'EMPI主索引'
                    },
                    {
                        value: 'card',
                        label: '身份证号'
                    }
                ],
                patientOverview: [],
                patientVisitList: [],
                patientVisitListCopy: [],
                patientEMRDetail: "",
                patientVisitListIndex: 1,
                patientVisitListTotal: 0,
                multiplePatientVisitSelection: [],

                patientHPOList: [],
                searchHPOText: "",
                searchHPODlg: false,
                searchedHPOList: [],
                searchedHPOListCopy: [],
                searchedHPOListTotal: 0,
                searchedHPOListIndex: 1,
                multipleSearchedHPOSelection: [],

                //normalDiseaseList: [],
                rareDiseaseList: [],

                nlpEngine: "String_Search",
                nlpEngineOptions: [
                    {
                        value: 'String_Search',
                        label: 'String Search'
                    },
                    {
                        value: 'Spacy',
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
                rareAnalyzeEngine: "Jaccard",
                rareAnalyzeEngineOptions: [
                    {
                        value: 'Jaccard',
                        label: 'Jaccard'
                    },
                    {
                        value: 'Tanimoto',
                        label: 'Tanimoto'
                    },
                    {
                        value: 'Overlap',
                        label: 'Overlap'
                    },             
                    {
                        value: 'Logolikelihood',
                        label: 'Logo-likelihood & ratio'
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
                       //for nlp engine
                    {
                        value: 'ORPHA',
                        label: 'ORPHANET'
                    },
                    {
                        value: 'DECIPHER',
                        label: 'DECIPHER'
                    },
                       //for nlp engine
                    {
                        value: 'all',
                        label: '整合库'
                    }
                ],
                clickedHPOText: "" ,// 一个HPO 有多次文本匹配，记住上次点击的那一个
                clickedHPOIndex:0
                
            };
        },
        template: "#v-disease-cdss",    
        methods: {
            onPopImportEMRDlg: function () {
                this.importEMRDlg = true;
            },
            onSearchPatient: function () {
                this.patientVisitListIndex = 1;
                this.requestPatientData();
                this.patientEMRDetail = "";
                this.multiplePatientVisitSelection = [];
            },
            requestPatientData: function () {
                var para = {};
                para = {
                    number: this.number,
                    numberType: this.numberType

                };
                const loading = this.$loading({
                    lock: true,
                    text: '拼命加载中...',
                    spinner: 'el-icon-loading',
                    background: 'rgba(0, 0, 0, 0.7)'
                });
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
                            that.patientVisitPaging();
                        }
                        else {
                            console.log(data);
                        }
                        loading.close();
                    }
                });
            },
            patientVisitPaging: function () {
                this.patientVisitList = this.localPaging(this.patientVisitListIndex, this.patientVisitListCopy);
            },
            onPatientVisitSelectionChange: function (val) {
                this.multiplePatientVisitSelection = val;
            },
            //根据所选取的就诊记录，查询数据
            onImportPatientEMRText: function () {
                var visitIds = this.multiplePatientVisitSelection.map(function (item) {
                    return item.visitid;
                });

                if (visitIds.length === 0) {
                    alert("请选择相应的就诊记录");
                    return;
                }
                this.patientEMRDetail = "";
                this.patientHPOList = [];
                var para = {};
                para = {
                    patientVisitIds: visitIds.toString()   
                };    
                const loading = this.$loading({
                    lock: true,
                    text: '拼命加载中...',
                    spinner: 'el-icon-loading',
                    background: 'rgba(0, 0, 0, 0.7)'
                });
                var that = this;
                $.ajax({
                    url: "/Home/GetPatientEMRDetail",
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
                        loading.close();
                    }
                });
                this.importEMRDlg = false;
            },
            onClearPatientEMR: function () {
                this.patientEMRDetail = "";
                this.patientHPOList = [];
                this.rareDiseaseList = [];
                this.$refs.multipleTable.clearSelection();
            },
            //这个地方有两种情况
            //1. 用户直接传一段文本，和病人没有关联，这个时候直接分析文本
            //2. 如果就诊记录 不为空，说明是获取刚才查询病人的HPO 结果，这个情况根据就诊记录直接从数据库获取
            onGetPatientHPOResult: function () {
                if (this.patientEMRDetail === undefined) {
                    alert('电子病历不能为空！');
                    return false;
                }
                this.patientHPOList = [];
                this.rareDiseaseList = [];
                var visitIds = this.multiplePatientVisitSelection.map(function (item) {
                    return item.visitid;
                });
                var para = {};
                
                para = {
                    patientEMRDetail:encodeURI(this.patientEMRDetail),
                    patientVisitIds: visitIds.toString(),
                    nlpEngine: this.nlpEngine
                };
                const loading = this.$loading({
                    lock: true,
                    text: '拼命加载中...',
                    spinner: 'el-icon-loading',
                    background: 'rgba(0, 0, 0, 0.7)'
                });
                var that = this;
                $.ajax({
                    url: "/Home/GetPatientHPOResult",
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
                        loading.close();
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
                //如果有多次匹配，那么循环显示
                var indexData;
                if (this.clickedHPOText === subgroup.hpoId) {
                    if (this.clickedHPOIndex < subgroup.indexList.length - 1) {
                        this.clickedHPOIndex++;
                    }
                    else {
                        this.clickedHPOIndex = 0;
                    }
                    indexData = subgroup.indexList[this.clickedHPOIndex];
                }
                else {
                    this.clickedHPOIndex = 0;
                    indexData = subgroup.indexList[0];
                    this.clickedHPOText = subgroup.hpoId;
                }
                var startIndex = indexData.startIndex;
                var endIndex = indexData.endIndex;

                var text = $('#txt_patientEMR').val();
                var textBeforePosition = text.substr(0, startIndex);
                $('#txt_patientEMR').blur();
                $('#txt_patientEMR').val(textBeforePosition);
                $('#txt_patientEMR').focus();
                $('#txt_patientEMR').val(text);
                $('#txt_patientEMR').selectRange(startIndex, endIndex); 

            },
            onSearchHPODlg: function () {
                this.searchHPODlg = true;
            },
            //查询表征HPO 库，自定义添加HPO
            onSearchHPOList: function () {
                this.searchedHPOListIndex = 1;
                var para = {};
                para = {
                    searchHPOText: this.searchHPOText
                };
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Home/SearchStandardHPOList",
                    type: "Get",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.searchedHPOListTotal = data.total;
                            that.searchedHPOListCopy = data.data;
                            that.HPOLocalPaging();
                        }
                        else {
                            console.log(data);
                        }
                        that.loading = false;
                    }
                });
            },
            HPOLocalPaging: function () {
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
            onGetPatientRareDiseaseResult: function () {
                if (this.rareAnalyzeEngine === undefined || this.rareAnalyzeEngine === "") {
                    alert('请选择罕见病分析引擎！');
                    return false;
                }
                if (this.rareDataBaseEngine === undefined || this.rareDataBaseEngine === "") {
                    alert('请选择罕见病数据库！');
                    return false;
                }
                this.rareDiseaseList = [];
                const loading = this.$loading({
                    lock: true,
                    text: '拼命加载中...',
                    spinner: 'el-icon-loading',
                    background: 'rgba(0, 0, 0, 0.7)'
                });
                var that = this;
                var para = {};
                para = {
                    hpoList: this.patientHPOList,
                    rareAnalyzeEngine: this.rareAnalyzeEngine,
                    rareDataBaseEngine: this.rareDataBaseEngine
                };
                $.ajax({
                    url: "/Home/GetPatientRareDiseaseResult",
                    type: "POST",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            //that.normalDiseaseList = data.normalDiseaseList;
                            that.rareDiseaseList = data.rareDiseaseList;
                        }
                        else {
                            console.log(data);
                        }
                        loading.close();
                    }
                });
            },
            statusFormatter(row, column) {
                let status = row.match;
                if (status === 0) {
                    return '否';
                } else {
                    return '是';
                }
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
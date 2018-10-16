// Property Editors
angular.module("umbraco").controller("Our.Umbraco.ShadowContentPicker.Controllers.PropertyEditorController", [

    "$scope",
    "editorState",
    "innerContentService",
    "Our.Umbraco.ShadowContentPicker.Resources",

    function ($scope, editorState, innerContentService, shadowContentResources) {

        var docTypes = [];

        var vm = this;
        vm.allowAdd = true;
        vm.allowEdit = true;
        vm.allowRemove = true;
        vm.published = true;
        vm.sortable = true;

        vm.sortableOptions = {
            axis: "y",
            containment: "parent",
            cursor: "move",
            disabled: vm.sortable === false,
            opacity: 0.7,
            scroll: true,
            tolerance: "pointer",
            stop: function (e, ui) {
                setDirty();
            }
        };

        vm.contentPickerOverlay = {
            currentNode: editorState ? editorState.current : null,
            entityType: "Document",
            filterCssClass: "not-allowed not-published",
            idType: "udi",
            multiPicker: true,
            section: "content",
            show: false,
            startNodeId: $scope.model.config.startNodeId,
            treeAlias: "content",
            view: "treepicker",
        };

        vm.innerContentOverlay = {
            propertyAlias: $scope.model.alias,
            contentTypes: docTypes,
            contentTypePickerItems: [],
            show: false,
            callback: function (data) {
                var overrides = [];
                if (this.editorModels) {
                    var currentEditorModel = _.find(this.editorModels, function (x) {
                        return x.icContentTypeGuid === data.model.icContentTypeGuid;
                    });

                    if (currentEditorModel && currentEditorModel.properties) {
                        var propertyAliases = _.difference(_.keys(data.model), ["key", "name", "icon", "icContentTypeGuid"]);
                        _.each(propertyAliases, function (x) {
                            var v = data.model[x];
                            if (_.isEmpty(data.model[x]) === false) {
                                var property = _.find(currentEditorModel.properties, function (p) {
                                    return p.alias === x;
                                });

                                overrides.push({ name: property ? property.label : x });
                            }
                        });
                    }
                }

                $scope.model.value[data.idx].content = data.model;
                $scope.model.value[data.idx].item.overrides = overrides;
            }
        };

        vm.add = add;
        vm.edit = edit;
        vm.remove = remove;

        function add($event) {
            vm.contentPickerOverlay.show = true;
            vm.contentPickerOverlay.submit = function (data) {
                if (angular.isArray(data.selection)) {
                    _.each(data.selection, function (entity, i) {

                        // TODO: I don't like hitting the API/db per loop iteration. Could we batch this? [LK]
                        shadowContentResources.getContentTypeGuidByAlias(entity.metaData.ContentTypeAlias).then(function (data) {

                            // check that it hasn't already been added
                            var docTypeExists = _.find(docTypes, function (x) {
                                return x.icContentTypeGuid === data.key;
                            });
                            if (docTypeExists === undefined) {
                                docTypes.push({ icContentTypeGuid: data.key });
                            }

                            // check that the node hasn't already been added
                            var nodeExists = _.find($scope.model.value, function (x) {
                                return x.item.id === entity.id;
                            });
                            if (nodeExists === undefined) {
                                $scope.model.value.push({
                                    udi: entity.udi,
                                    contentTypeGuid: data.key,
                                    item: {
                                        name: entity.name,
                                        id: entity.id,
                                        icon: entity.icon,
                                        path: entity.path,
                                        trashed: entity.trashed,
                                        published: entity.metaData && entity.metaData.IsPublished,
                                        overrides: null
                                    },
                                    content: {}
                                });
                            }

                        });

                    });
                }

                vm.contentPickerOverlay.show = false;

                setDirty();
            };
            vm.contentPickerOverlay.close = function () {
                vm.contentPickerOverlay.show = false;
            };
        };

        function edit($event, $index, item) {
            if (_.isEmpty(item.content)) {
                item.content = {}
            }

			item.content.icContentTypeGuid = item.content.icContentTypeGuid || item.contentTypeGuid;
			item.content.name = item.content.name || item.item.name;

            vm.innerContentOverlay.event = $event;
            vm.innerContentOverlay.data = { model: item.content, idx: $index, action: "edit" };
            vm.innerContentOverlay.show = true;
        };

        function remove($index) {
            $scope.model.value.splice($index, 1);
            setDirty();
        };

        function setDirty() {
            if ($scope.propertyForm) {
                $scope.propertyForm.$setDirty();
            }
        };

        function initialize() {
            $scope.model.value = $scope.model.value || [];

            if ($scope.model.value.length > 0) {
                _.each($scope.model.value, function (x) {
                    if (x.contentTypeGuid) {
                        docTypes.push({ icContentTypeGuid: x.contentTypeGuid });
                    }
                });
            }
        }

        initialize();
    }

]);

// Resources
angular.module("umbraco.resources").factory("Our.Umbraco.ShadowContentPicker.Resources", [

    "$http",
    "umbRequestHelper",

    function ($http, umbRequestHelper) {
        return {
            getContentTypeGuidByAlias: function (alias) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: umbRequestHelper.convertVirtualToAbsolutePath("~/umbraco/backoffice/ShadowContentPicker/ShadowContentPickerApi/GetContentTypeGuidByAlias"),
                        method: "GET",
                        params: { alias: alias }
                    }),
                    "Failed to retrieve Content Type Guid by Alias"
                );
            }
        };
    }
]);
var Select2Ajax = function (id, url, pageSize, minInput) {
    if (typeof pageSize === 'undefined') { pageSize = 10; }
    if (typeof minInput === 'undefined') { minInput = 2; }

    $(id).select2({
        ajax: {
            url: url,
            dataType: 'json',
            delay: 250,
            data: function (params) {
                if (typeof params.page === 'undefined') { params.page = 0; }
                if (typeof params.pageSize === 'undefined') { params.pageSize = pageSize; }

                return {
                    search: params.term,
                    pageNumber: params.page + 1,
                    pageSize: params.pageSize
                };
            },
            processResults: function (data, params) {
                var result = data.result;
                return {
                    results: result.items,
                    pagination: {
                        more: result.more
                    }
                };
            }
        },
        minimumInputLength: minInput
    });
};
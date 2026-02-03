window.docPreview = {
    renderWord: async function (elementId, arrayBuffer) {
        var options = {
            styleMap: [
                "p[style-name='Section Title'] => h1:fresh",
                "p[style-name='Subsection Title'] => h2:fresh"
            ]
        };
        
        try {
            var result = await mammoth.convertToHtml({ arrayBuffer: arrayBuffer }, options);
            var displayResult = result.value; // The generated HTML
            var messages = result.messages; // Any warnings
            
            var element = document.getElementById(elementId);
            if (element) {
                element.innerHTML = displayResult;
            }
        } catch (error) {
            console.error(error);
            var element = document.getElementById(elementId);
            if (element) {
                element.innerHTML = '<div class="alert alert-danger">Failed to render Word document: ' + error.message + '</div>';
            }
        }
    },

    renderExcel: function (elementId, arrayBuffer) {
        try {
            var data = new Uint8Array(arrayBuffer);
            var workbook = XLSX.read(data, { type: 'array' });
            
            // Get first sheet
            var firstSheetName = workbook.SheetNames[0];
            var worksheet = workbook.Sheets[firstSheetName];
            
            // Convert to HTML
            var html = XLSX.utils.sheet_to_html(worksheet, { id: "excel-table", editable: false });
            
            var element = document.getElementById(elementId);
            if (element) {
                element.innerHTML = html;
            }
        } catch (error) {
            console.error(error);
            var element = document.getElementById(elementId);
            if (element) {
                element.innerHTML = '<div class="alert alert-danger">Failed to render Excel document: ' + error.message + '</div>';
            }
        }
    }
};

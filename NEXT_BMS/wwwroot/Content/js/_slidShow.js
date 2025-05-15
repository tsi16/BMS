function LoadSlidShow(id) {
    $.ajax({
        cache: false,
        type: 'POST',
        url: '@Url.Action("GetSlidShowImages", "Shops")', 
        data: { 'id': id },
        async: true,
        success: function (data) {
            console.log("Data retrieved:", data); // Log the data for inspection

            // Clear the previous carousel content
            $(".carousel-inner").empty();
            $(".carousel-indicators").empty();

            if (!data || data.length === 0) {
                console.error("No data returned from the server.");
                $("#carouselExampleCaptions").hide(); // Hide carousel if no data
                return; // If there's no data, exit early
            }

            let carouselInner = "";
            let indicators = "";

            $(data).each(function (index, item) {
                // Create a new carousel item
                if (index === 0) {
                    carouselInner += `<div class="carousel-item active">`;
                } else {
                    carouselInner += `<div class="carousel-item">`;
                }

                // Image and description
                carouselInner += `
                    <div class="text-center">
                        <img src="${item.imageUrl}" class="d-block w-100" style="height:500px;" 
                             onerror="this.onerror=null; this.src='/assets/img/default.jpg';" /> <!-- Fallback image -->
                        <p>${item.description}</p>
                    </div>
                `;
                carouselInner += `</div>`; // Close carousel item

                // Create indicator buttons
                let clsnameIndicator = index === 0 ? "active" : "";
                indicators += `
                    <button type="button" data-bs-target="#carouselExampleCaptions" data-bs-slide-to="${index}" class="${clsnameIndicator}" aria-label="Slide ${index + 1}"></button>`;
            });

            // Append carousel items and indicators
            $(".carousel-inner").append(carouselInner);
            $(".carousel-indicators").append(indicators);

            $('#carouselExampleCaptions').carousel();

            // Show or hide carousel based on image count
            if (data.length <= 0) {
                $("#carouselExampleCaptions").hide();
            } else {
                $("#carouselExampleCaptions").show();
            }
        },
        error: function (xhr, status, error) {
            console.error("Error loading images: ", error);
        }
    });
}
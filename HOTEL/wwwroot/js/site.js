// Add this script to your site.js file or as a separate script file to include globally

/**
 * Handle Book Now button clicks across the site
 * @param {number} roomId - The ID of the room being booked
 */
function handleBookNowClick(roomId) {
    const isLoggedIn = document.querySelector('meta[name="isLoggedIn"]').content === 'true';
    
    if (isLoggedIn) {
        // User is logged in, redirect to booking page
        window.location.href = `/Booking/Create?roomId=${roomId}`;
    } else {
        // User is not logged in, show login modal
        showLoginModal(roomId);
    }
}

/**
 * Additional utility to check if user is logged in
 * @returns {boolean} - Whether the user is logged in
 */
function isUserLoggedIn() {
    return document.querySelector('meta[name="isLoggedIn"]').content === 'true';
}

function showLoginModal(roomId) {
    const loginModal = new bootstrap.Modal(document.getElementById('loginModal'));
    document.getElementById('returnUrl').value = `/Booking/Create/${roomId}`;
    document.getElementById('roomId').value = roomId;
    loginModal.show();
}
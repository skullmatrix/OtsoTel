/**
 * Animation helper script for The Matrix Hotel website
 * Provides transition-based animations without using @keyframes
 */

document.addEventListener('DOMContentLoaded', function() {
    // Initialize animations
    initFadeAnimations();
    
    // Setup intersection observers for scroll-based animations
    setupScrollAnimations();
});

/**
 * Initialize fade animations for elements that should animate immediately
 */
function initFadeAnimations() {
    // Get all elements with fade-in class
    const fadeElements = document.querySelectorAll('.fade-in, .slide-in, .fade-in-up');
    
    // Add show class with a small delay to trigger transitions
    setTimeout(() => {
        fadeElements.forEach(element => {
            element.classList.add('show');
        });
    }, 100);
}

/**
 * Setup intersection observers for scroll-based animations
 */
function setupScrollAnimations() {
    // Elements to animate on scroll
    const animatedElements = document.querySelectorAll('.animate-on-scroll');
    
    if (animatedElements.length === 0) return;
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                // Add a staggered delay for sequential animations
                setTimeout(() => {
                    entry.target.classList.add('show');
                }, index * 150);
                
                // Unobserve once animation is triggered
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.2 }); // Trigger when 20% of element is visible
    
    // Observe each element
    animatedElements.forEach(element => {
        observer.observe(element);
    });
}

/**
 * Helper function to add animation classes to an element
 * @param {HTMLElement} element - Element to animate
 * @param {string} animationType - Type of animation (fade-in, slide-in, fade-in-up)
 * @param {boolean} animateOnScroll - Whether to animate on scroll or immediately
 */
function animateElement(element, animationType, animateOnScroll = false) {
    if (!element) return;
    
    // Add animation class
    element.classList.add(animationType);
    
    if (animateOnScroll) {
        element.classList.add('animate-on-scroll');
    } else {
        // Trigger animation immediately
        setTimeout(() => {
            element.classList.add('show');
        }, 100);
    }
}
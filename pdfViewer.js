// secureFileViewer.js - Complete file viewer functionality (FIXED VERSION)

// ==================== PDF.JS INTEGRATION ====================
// Add to your site.js or create a new utility file
window.showCourseSelectionModal = function (courses, dotNetHelper) {
    // Create modal HTML
    const modalHtml = `
        <div class="modal fade" id="courseSelectionModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Select a Course</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <p class="mb-3">Select which course to add the lecture to:</p>
                        <div class="list-group">
                            ${courses.map(course => `
                                <button type="button" 
                                        class="list-group-item list-group-item-action"
                                        onclick="selectCourse(${course.id})">
                                    <div class="d-flex align-items-center">
                                        ${course.thumbnail ? `<img src="${course.thumbnail}" class="rounded me-3" width="50" height="50">` : ''}
                                        <div>
                                            <h6 class="mb-0">${course.title}</h6>
                                        </div>
                                    </div>
                                </button>
                            `).join('')}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Add modal to body
    document.body.insertAdjacentHTML('beforeend', modalHtml);

    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('courseSelectionModal'));
    modal.show();

    // Store the dotnet helper globally for the button click
    window.selectCourse = function (courseId) {
        modal.hide();
        dotNetHelper.invokeMethodAsync('OnCourseSelectedForLecture', courseId);

        // Cleanup
        document.getElementById('courseSelectionModal').remove();
        delete window.selectCourse;
    };

    // Cleanup on modal close
    document.getElementById('courseSelectionModal').addEventListener('hidden.bs.modal', function () {
        this.remove();
        delete window.selectCourse;
    });
};
function downloadCSV(filename, csvContent) {
    var blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    var link = document.createElement("a");

    if (link.download !== undefined) {
        var url = URL.createObjectURL(blob);
        link.setAttribute("href", url);
        link.setAttribute("download", filename);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
}
function initializeSortable() {
    const sortableList = document.getElementById('sortable-list');

    if (sortableList) {
        new Sortable(sortableList, {
            animation: 150,
            ghostClass: 'sortable-ghost',
            chosenClass: 'sortable-chosen',
            dragClass: 'sortable-drag',
            onEnd: function (evt) {
                console.log('Item moved', evt.oldIndex, '->', evt.newIndex);
            }
        });
    }
}

// Get sorted IDs from sortable list
function getSortedIds() {
    const items = document.querySelectorAll('.sortable-item');
    const ids = [];

    items.forEach(item => {
        const id = item.getAttribute('data-id');
        if (id) {
            ids.push(parseInt(id));
        }
    });

    return ids;
}
// Check if PDF.js is loaded
window.checkPdfJsReady = () => {
    console.log('Checking if PDF.js is ready...');
    return typeof pdfjsLib !== 'undefined' && typeof pdfjsLib.getDocument !== 'undefined';
};

// Load PDF.js library dynamically
window.loadPdfJsLibrary = async () => {
    console.log('Loading PDF.js library...');

    // Check if already loaded
    if (typeof pdfjsLib !== 'undefined') {
        console.log('PDF.js already loaded');
        return true;
    }

    try {
        // Load PDF.js main library
        await loadScript('https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js');
        console.log('PDF.js main library loaded');

        // Configure PDF.js worker
        if (typeof pdfjsLib !== 'undefined') {
            pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
            console.log('PDF.js worker configured');
        }

        return true;
    } catch (error) {
        console.error('Failed to load PDF.js:', error);
        return false;
    }
};

// Add these variables at the top of your JavaScript file
let cachedPdfData = null;
let currentPdfDocument = null;
let currentPdfRenderTask = null;

// Clean up PDF.js resources
window.cleanupPdfJs = () => {
    try {
        // Cancel any ongoing render task
        if (currentPdfRenderTask) {
            currentPdfRenderTask.cancel();
            currentPdfRenderTask = null;
            console.log('PDF.js render task cancelled');
        }

        // Clear canvas
        const canvas = document.getElementById('pdf-canvas');
        if (canvas) {
            const ctx = canvas.getContext('2d');
            if (ctx) {
                ctx.clearRect(0, 0, canvas.width, canvas.height);
            }
            canvas.width = 0;
            canvas.height = 0;
        }

        // Note: We DON'T destroy the PDF document here anymore
        // It will be reused for page navigation

        return true;
    } catch (error) {
        console.error('Error cleaning up PDF.js:', error);
        return false;
    }
};

// Destroy PDF document when switching materials
window.destroyPdfDocument = () => {
    try {
        if (currentPdfDocument) {
            currentPdfDocument.destroy();
            currentPdfDocument = null;
            cachedPdfData = null;
            console.log('PDF document destroyed');
        }
        return true;
    } catch (error) {
        console.error('Error destroying PDF document:', error);
        return false;
    }
};
// Load PDF once and return a reference
window.loadPdfDocument = async function (base64Data) {
    try {
        console.log('Loading PDF document for caching...');

        // Convert base64 to Uint8Array
        const binaryString = atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        // Load PDF document
        const loadingTask = pdfjsLib.getDocument({ data: bytes });
        window.currentPdfDocument = await loadingTask.promise;

        console.log(`PDF loaded successfully. Total pages: ${window.currentPdfDocument.numPages}`);

        // Return a reference ID (could be a simple string)
        return 'pdf-loaded-' + Date.now();

    } catch (error) {
        console.error('Error loading PDF document:', error);
        throw error;
    }
};

// Get total pages from already loaded PDF
window.getPdfTotalPagesFromReference = function (pdfRef) {
    if (window.currentPdfDocument) {
        return window.currentPdfDocument.numPages;
    }
    throw new Error('PDF document not loaded');
};
// Render page from already loaded PDF
window.renderPageFromLoadedPdf = async function (canvasId, pageNumber, scale) {
    console.log(`Rendering page ${pageNumber} from loaded PDF...`);

    try {
        if (!window.currentPdfDocument) {
            throw new Error('PDF document not loaded. Call loadPdfDocument first.');
        }

        const canvas = document.getElementById(canvasId);
        if (!canvas || !canvas.getContext) {
            throw new Error(`Canvas not found: ${canvasId}`);
        }

        const pdf = window.currentPdfDocument;

        // Validate page number
        if (pageNumber < 1 || pageNumber > pdf.numPages) {
            throw new Error(`Page ${pageNumber} out of range (1-${pdf.numPages})`);
        }

        const ctx = canvas.getContext('2d');

        // Get the requested page
        console.log(`Extracting page ${pageNumber}...`);
        const page = await pdf.getPage(pageNumber);

        // Get viewport with scale
        const viewport = page.getViewport({ scale: scale });

        // Set canvas dimensions
        canvas.width = viewport.width;
        canvas.height = viewport.height;

        // Clear canvas and set white background
        ctx.fillStyle = 'white';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Render the page
        console.log(`Rendering to canvas (${viewport.width}x${viewport.height})...`);
        const renderContext = {
            canvasContext: ctx,
            viewport: viewport
        };

        await page.render(renderContext);
        console.log(`Page ${pageNumber} rendered successfully`);

        return true;

    } catch (error) {
        console.error('Error rendering page from loaded PDF:', error);
        throw error;
    }
};
// Update the renderPdfPage function to use the new variables
// Update the renderPdfPage function
window.renderPdfPage = async (canvasId, base64Data, pageNumber, scale) => {
    console.log(`Starting PDF render - Canvas ID: ${canvasId}, Page: ${pageNumber}, Scale: ${scale}`);

    try {
        // Check if we already have this PDF loaded
        if (!window.cachedPdfData || window.cachedPdfData !== base64Data) {
            console.log('New PDF detected or no cached PDF, cleaning up...');
            // Clean up previous PDF if exists
            await window.cleanupPdfJs();

            // Cache the new PDF data
            window.cachedPdfData = base64Data;
            console.log('Caching new PDF data');

            // Reset the PDF document
            window.currentPdfDocument = null;
        } else {
            console.log('Using cached PDF data');
        }

        // Get canvas
        const canvas = document.getElementById(canvasId);
        if (!canvas || !canvas.getContext) {
            throw new Error(`Canvas not found or not supported: ${canvasId}`);
        }

        if (!base64Data || base64Data.length === 0) {
            throw new Error('PDF data is empty');
        }

        if (pageNumber < 1) {
            throw new Error('Page number must be at least 1');
        }

        // Check if PDF.js is available
        if (typeof pdfjsLib === 'undefined') {
            throw new Error('PDF.js library not loaded');
        }

        const ctx = canvas.getContext('2d');

        // Load PDF document if not already loaded
        if (!window.currentPdfDocument) {
            console.log('Loading PDF document...');

            // Convert base64 to Uint8Array
            const binaryString = atob(base64Data);
            const bytes = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) {
                bytes[i] = binaryString.charCodeAt(i);
            }

            const loadingTask = pdfjsLib.getDocument({ data: bytes });
            window.currentPdfDocument = await loadingTask.promise;

            console.log(`PDF loaded successfully. Total pages: ${window.currentPdfDocument.numPages}`);
        } else {
            console.log(`Using existing PDF document with ${window.currentPdfDocument.numPages} pages`);
        }

        const pdf = window.currentPdfDocument;

        if (!pdf) {
            throw new Error('PDF document not loaded');
        }

        // Validate page number
        if (pageNumber > pdf.numPages) {
            throw new Error(`Page ${pageNumber} exceeds total pages (${pdf.numPages})`);
        }

        // Cancel any ongoing render task
        if (window.currentPdfRenderTask) {
            window.currentPdfRenderTask.cancel();
            console.log('Cancelled previous render task');
        }

        // Get the requested page
        console.log(`Loading page ${pageNumber}...`);
        const page = await pdf.getPage(pageNumber);

        // Get viewport with scale
        const viewport = page.getViewport({ scale: scale });
        console.log(`Viewport dimensions: ${viewport.width}x${viewport.height}`);

        // Set canvas dimensions
        canvas.width = viewport.width;
        canvas.height = viewport.height;

        // Clear canvas and set white background
        ctx.fillStyle = 'white';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Render the page
        console.log('Rendering page to canvas...');
        const renderContext = {
            canvasContext: ctx,
            viewport: viewport
        };

        const renderTask = page.render(renderContext);
        window.currentPdfRenderTask = renderTask;

        await renderTask.promise;
        console.log('Page rendered successfully');

        // Clear the render task reference
        window.currentPdfRenderTask = null;

        return pdf.numPages;

    } catch (error) {
        console.error('Error in renderPdfPage:', error);

        // Don't cleanup on page navigation errors, just the render task
        if (window.currentPdfRenderTask) {
            try {
                window.currentPdfRenderTask.cancel();
            } catch (e) {
                // Ignore
            }
            window.currentPdfRenderTask = null;
        }

        throw error;
    }
};

// Get total number of pages in PDF
// Get total number of pages in PDF
window.getPdfTotalPages = async (base64Data) => {
    console.log('Getting PDF total pages...');

    try {
        if (!base64Data || base64Data.length === 0) {
            throw new Error('PDF data is empty');
        }

        // Check if PDF.js is available
        if (typeof pdfjsLib === 'undefined') {
            throw new Error('PDF.js library not loaded');
        }

        // If we already have the document loaded with the same data, use it
        if (window.currentPdfDocument && window.cachedPdfData === base64Data) {
            console.log(`Using cached PDF, total pages: ${window.currentPdfDocument.numPages}`);
            return window.currentPdfDocument.numPages;
        }

        // Otherwise load just to get page count
        const binaryString = atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        // Load PDF document to get page count
        const loadingTask = pdfjsLib.getDocument({ data: bytes });
        const pdf = await loadingTask.promise;

        console.log(`PDF has ${pdf.numPages} pages`);

        // Clean up the temporary document
        pdf.destroy();

        return pdf.numPages;

    } catch (error) {
        console.error('Error in getPdfTotalPages:', error);
        throw error;
    }
};
// Get canvas as base64 image
window.getCanvasAsBase64 = function (canvasId) {
    try {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return null;

        // Get canvas data as PNG
        return canvas.toDataURL('image/png');
    } catch (error) {
        console.error('Error getting canvas as base64:', error);
        return null;
    }
};

// Display cached canvas
window.displayCachedCanvas = function (canvasId, base64Image) {
    try {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return false;

        const ctx = canvas.getContext('2d');
        if (!ctx) return false;

        // Create image from base64
        const img = new Image();
        img.onload = function () {
            // Clear canvas
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            // Draw cached image
            ctx.drawImage(img, 0, 0);
            console.log('Cached canvas displayed');
        };

        img.src = base64Image;
        return true;

    } catch (error) {
        console.error('Error displaying cached canvas:', error);
        return false;
    }
};

// Pre-render page to hidden canvas
window.preRenderPdfPage = async function (hiddenCanvasId, base64Data, pageNumber, scale) {
    try {
        const canvas = document.getElementById(hiddenCanvasId);
        if (!canvas || !canvas.getContext) {
            console.error('Hidden canvas not found');
            return null;
        }

        if (!base64Data || base64Data.length === 0) {
            throw new Error('PDF data is empty');
        }

        if (pageNumber < 1) {
            throw new Error('Page number must be at least 1');
        }

        // Check if PDF.js is available
        if (typeof pdfjsLib === 'undefined') {
            throw new Error('PDF.js library not loaded');
        }

        const ctx = canvas.getContext('2d');

        // Load PDF document if not already loaded
        if (!window.currentPdfDocument) {
            console.log('Loading PDF document for pre-render...');

            // Convert base64 to Uint8Array
            const binaryString = atob(base64Data);
            const bytes = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) {
                bytes[i] = binaryString.charCodeAt(i);
            }

            const loadingTask = pdfjsLib.getDocument({ data: bytes });
            window.currentPdfDocument = await loadingTask.promise;
        }

        const pdf = window.currentPdfDocument;

        if (!pdf) {
            throw new Error('PDF document not loaded');
        }

        // Validate page number
        if (pageNumber > pdf.numPages) {
            throw new Error(`Page ${pageNumber} exceeds total pages (${pdf.numPages})`);
        }

        // Get the requested page
        const page = await pdf.getPage(pageNumber);

        // Get viewport with scale
        const viewport = page.getViewport({ scale: scale });

        // Set canvas dimensions
        canvas.width = viewport.width;
        canvas.height = viewport.height;

        // Clear canvas and set white background
        ctx.fillStyle = 'white';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Render the page
        const renderContext = {
            canvasContext: ctx,
            viewport: viewport
        };

        await page.render(renderContext);

        // Get as base64
        const base64Image = canvas.toDataURL('image/png');

        console.log(`Pre-rendered page ${pageNumber} successfully`);
        return base64Image;

    } catch (error) {
        console.error('Error in preRenderPdfPage:', error);
        return null;
    }
};
// Get total number of pages in PDF
window.getPdfTotalPages = async (base64Data) => {
    console.log('Getting PDF total pages...');

    try {
        if (!base64Data || base64Data.length === 0) {
            throw new Error('PDF data is empty');
        }

        // Check if PDF.js is available
        if (typeof pdfjsLib === 'undefined') {
            throw new Error('PDF.js library not loaded');
        }

        // Convert base64 to Uint8Array
        const binaryString = atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        // Load PDF document to get page count
        const loadingTask = pdfjsLib.getDocument({ data: bytes });
        const pdf = await loadingTask.promise;

        console.log(`PDF has ${pdf.numPages} pages`);
        return pdf.numPages;

    } catch (error) {
        console.error('Error in getPdfTotalPages:', error);
        throw error;
    }
};

// Initialize PDF.js
window.initializePdfJs = () => {
    if (typeof pdfjsLib !== 'undefined') {
        pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
        console.log('PDF.js initialized with worker');
        return true;
    }
    return false;
};

// ==================== SECURE VIEWER FUNCTIONS ====================

// Initialize secure viewer
window.initializeSecureViewer = (elementRef) => {
    try {
        const element = elementRef;
        if (!element) {
            console.warn('Element not found for secure viewer initialization');
            return;
        }

        console.log('Initializing secure viewer...');

        // Disable right-click context menu
        element.addEventListener('contextmenu', (e) => {
            e.preventDefault();
            return false;
        });

        // Disable keyboard shortcuts for save, print, etc.
        element.addEventListener('keydown', (e) => {
            // Disable Ctrl+S, Ctrl+P, etc.
            if ((e.ctrlKey || e.metaKey) &&
                (e.key === 's' || e.key === 'p' || e.key === 'S' || e.key === 'P')) {
                e.preventDefault();
                return false;
            }

            // Disable F12 for developer tools
            if (e.key === 'F12') {
                e.preventDefault();
                return false;
            }

            // Disable print screen
            if (e.key === 'PrintScreen') {
                e.preventDefault();
                return false;
            }
        });

        // Prevent drag and drop of images/content
        element.addEventListener('dragstart', (e) => {
            if (e.target.tagName === 'IMG' || e.target.tagName === 'CANVAS') {
                e.preventDefault();
                return false;
            }
        });

        console.log('Secure viewer initialized');

    } catch (error) {
        console.error('Error initializing secure viewer:', error);
    }
};

// Disable context menu for an element
window.disableContextMenu = (elementRef) => {
    try {
        const element = elementRef;
        if (element) {
            element.addEventListener('contextmenu', (e) => {
                e.preventDefault();
                return false;
            }, false);
        }
    } catch (error) {
        console.error('Error disabling context menu:', error);
    }
};

// Scroll tracking helper
window.getScrollPercentage = function (element) {
    if (!element) return 0;

    const scrollTop = element.scrollTop;
    const scrollHeight = element.scrollHeight;
    const clientHeight = element.clientHeight;

    if (scrollHeight === clientHeight) return 1; // All content visible

    const scrolled = scrollTop + clientHeight;
    const percentage = scrolled / scrollHeight;

    return Math.min(1, Math.max(0, percentage));
};
// Pre-load PDF pages for faster navigation
window.preloadPdfPage = async (base64Data, pageNumber) => {
    try {
        // Don't pre-load if we're not on the same PDF
        if (cachedPdfData !== base64Data) {
            return false;
        }

        // Don't pre-load if document isn't loaded
        if (!currentPdfDocument) {
            return false;
        }

        // Validate page number
        if (pageNumber < 1 || pageNumber > currentPdfDocument.numPages) {
            return false;
        }

        // Pre-load the page (get it but don't render)
        await currentPdfDocument.getPage(pageNumber);
        console.log(`Pre-loaded page ${pageNumber}`);
        return true;

    } catch (error) {
        console.error(`Error pre-loading page ${pageNumber}:`, error);
        return false;
    }
};
// ==================== VIDEO PLAYER FUNCTIONS - FIXED ====================

// Video functions - SIMPLIFIED (No event dispatching)
// ==================== VIDEO PLAYER FUNCTIONS - FIXED ====================

// Video functions with proper error handling and state management

window.pauseVideo = function (videoElement) {
    try {
        if (!videoElement || !videoElement.pause) {
            console.warn('Video element or pause method not available');
            return false;
        }

        // Check if already paused
        if (videoElement.paused) {
            console.log('Video is already paused');
            return true;
        }

        // Clear any pending play promise
        if (window.videoPlayPromise) {
            window.videoPlayPromise = null;
        }

        // Pause the video
        videoElement.pause();
        console.log('Video paused successfully');
        return true;

    } catch (error) {
        console.error('Error pausing video:', error);
        return false;
    }
};

// Add a safe toggle function
window.toggleVideoPlay = async function (videoElement) {
    try {
        if (!videoElement) return false;

        if (videoElement.paused) {
            return await window.playVideo(videoElement);
        } else {
            return window.pauseVideo(videoElement);
        }
    } catch (error) {
        console.error('Error toggling video:', error);
        return false;
    }
};

// Add this helper to check video state
window.getVideoState = function (videoElement) {
    try {
        if (!videoElement) return 'no-element';

        return {
            paused: videoElement.paused,
            ended: videoElement.ended,
            currentTime: videoElement.currentTime,
            duration: videoElement.duration,
            readyState: videoElement.readyState,
            networkState: videoElement.networkState
        };
    } catch (error) {
        console.error('Error getting video state:', error);
        return 'error';
    }
};

window.setVideoMuted = function (videoElement, muted) {
    try {
        if (videoElement) {
            videoElement.muted = muted;
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error setting video muted:', error);
        return false;
    }
};

// Reload video with time parameter
window.reloadVideoWithTime = async function (videoElement, time) {
    try {
        console.log(`Reloading video with time: ${time}s`);

        // Store current state
        const currentSrc = videoElement.src;
        const wasPlaying = !videoElement.paused;

        // Add time parameter to URL
        let newSrc = currentSrc;
        const timeParam = `t=${Math.floor(time)}`;

        if (currentSrc.includes('?')) {
            newSrc = currentSrc.replace(/([?&])t=[^&]*/, `$1${timeParam}`);
            if (!newSrc.includes('t=')) {
                newSrc += `&${timeParam}`;
            }
        } else {
            newSrc = `${currentSrc}?${timeParam}`;
        }

        // Also add cache buster
        newSrc += `&_=${Date.now()}`;

        // Change source and reload
        videoElement.src = newSrc;
        videoElement.load();

        // Wait for video to load
        await new Promise((resolve) => {
            const onCanPlay = () => {
                videoElement.removeEventListener('canplay', onCanPlay);
                resolve();
            };
            videoElement.addEventListener('canplay', onCanPlay, { once: true });
        });

        // Set the exact time
        videoElement.currentTime = time;
        await new Promise(resolve => setTimeout(resolve, 300));

        // Resume if was playing
        if (wasPlaying) {
            await new Promise(resolve => setTimeout(resolve, 200));
            try {
                await videoElement.play();
            } catch (e) {
                console.log('Could not resume after reload:', e);
            }
        }

        return true;

    } catch (error) {
        console.error('Error reloading video:', error);
        return false;
    }
};

// Simulate a click on an element
window.simulateClick = function (element) {
    try {
        if (element) {
            element.click();
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error simulating click:', error);
        return false;
    }
};

// Enhanced video seeking with proper promise handling
// Enhanced video seeking with buffering support
window.setVideoCurrentTime = async function (videoElement, time) {
    try {
        if (!videoElement || isNaN(time)) {
            console.error('Invalid video element or time');
            return false;
        }

        console.log(`=== SET VIDEO TIME: ${time}s ===`);
        console.log('Video state:', {
            duration: videoElement.duration,
            currentTime: videoElement.currentTime,
            paused: videoElement.paused,
            readyState: videoElement.readyState,
            networkState: videoElement.networkState,
            buffered: videoElement.buffered.length > 0 ?
                `${videoElement.buffered.start(0)}-${videoElement.buffered.end(0)}` : 'empty',
            seekable: videoElement.seekable.length > 0 ?
                `${videoElement.seekable.start(0)}-${videoElement.seekable.end(0)}` : 'empty'
        });

        // Validate time is within seekable range
        if (videoElement.seekable.length > 0) {
            const seekableStart = videoElement.seekable.start(0);
            const seekableEnd = videoElement.seekable.end(0);

            if (time < seekableStart || time > seekableEnd) {
                console.warn(`Time ${time}s is outside seekable range ${seekableStart}-${seekableEnd}s`);
                time = Math.max(seekableStart, Math.min(seekableEnd, time));
                console.log(`Adjusted time to: ${time}s`);
            }
        }

        // Check if video is ready for seeking
        if (videoElement.readyState < 2) { // HAVE_CURRENT_DATA or higher
            console.warn('Video not ready for seeking (readyState:', videoElement.readyState, ')');

            // Wait for video to load more data
            console.log('Waiting for video to load...');
            await new Promise((resolve, reject) => {
                const onLoadedData = () => {
                    videoElement.removeEventListener('loadeddata', onLoadedData);
                    videoElement.removeEventListener('error', onError);
                    console.log('Video data loaded, readyState:', videoElement.readyState);
                    resolve();
                };

                const onError = () => {
                    videoElement.removeEventListener('loadeddata', onLoadedData);
                    videoElement.removeEventListener('error', onError);
                    reject(new Error('Video load error'));
                };

                videoElement.addEventListener('loadeddata', onLoadedData, { once: true });
                videoElement.addEventListener('error', onError, { once: true });

                // Timeout after 3 seconds
                setTimeout(() => {
                    videoElement.removeEventListener('loadeddata', onLoadedData);
                    videoElement.removeEventListener('error', onError);
                    console.log('Video load timeout, proceeding anyway');
                    resolve();
                }, 3000);
            });
        }

        // Store playing state
        const wasPlaying = !videoElement.paused;
        console.log('Was playing:', wasPlaying);

        // Pause if playing
        if (wasPlaying) {
            console.log('Pausing video...');
            videoElement.pause();
            await new Promise(resolve => setTimeout(resolve, 50));
        }

        // Method 1: Direct assignment (should work in most cases)
        console.log(`Setting currentTime to ${time}s...`);
        videoElement.currentTime = time;

        // Wait for time to update
        await new Promise(resolve => setTimeout(resolve, 100));

        // Check result
        let actualTime = videoElement.currentTime;
        console.log(`Actual time after Method 1: ${actualTime}s`);

        // If time wasn't set correctly, try alternative methods
        if (Math.abs(actualTime - time) > 0.5) {
            console.warn(`Time mismatch! Expected: ${time}s, Got: ${actualTime}s`);

            // Method 2: Use fastSeek if available (for MP4 videos)
            if (typeof videoElement.fastSeek === 'function') {
                console.log('Trying fastSeek...');
                try {
                    videoElement.fastSeek(time);
                    await new Promise(resolve => setTimeout(resolve, 100));
                    actualTime = videoElement.currentTime;
                    console.log(`Actual time after fastSeek: ${actualTime}s`);
                } catch (fastSeekError) {
                    console.log('fastSeek failed:', fastSeekError);
                }
            }

            // Method 3: Force reload for streaming videos
            if (Math.abs(actualTime - time) > 0.5 && time > 0) {
                console.log('Trying to force seek via src change...');
                try {
                    // Store current src
                    const currentSrc = videoElement.src;
                    const currentType = videoElement.getAttribute('type');

                    // Temporarily change src to force seeking
                    videoElement.src = currentSrc + (currentSrc.includes('?') ? '&' : '?') +
                        't=' + Math.floor(time) + '&_' + Date.now();

                    if (currentType) {
                        videoElement.setAttribute('type', currentType);
                    }

                    await new Promise(resolve => {
                        const onCanPlay = () => {
                            videoElement.removeEventListener('canplay', onCanPlay);
                            resolve();
                        };
                        videoElement.addEventListener('canplay', onCanPlay, { once: true });

                        // Load the video
                        videoElement.load();
                    });

                    // Set time again
                    videoElement.currentTime = time;
                    await new Promise(resolve => setTimeout(resolve, 300));

                    actualTime = videoElement.currentTime;
                    console.log(`Actual time after src change: ${actualTime}s`);

                    // Restore original src if still not correct
                    if (Math.abs(actualTime - time) > 0.5) {
                        console.log('Restoring original src...');
                        videoElement.src = currentSrc;
                        if (currentType) {
                            videoElement.setAttribute('type', currentType);
                        }
                        videoElement.load();
                        await new Promise(resolve => setTimeout(resolve, 200));
                        videoElement.currentTime = time;
                        await new Promise(resolve => setTimeout(resolve, 200));
                        actualTime = videoElement.currentTime;
                        console.log(`Actual time after restore: ${actualTime}s`);
                    }

                } catch (srcError) {
                    console.error('Error with src change method:', srcError);
                }
            }
        }

        // Resume if was playing
        if (wasPlaying && actualTime >= 0) {
            console.log('Attempting to resume playback...');
            await new Promise(resolve => setTimeout(resolve, 200));

            try {
                await videoElement.play();
                console.log('Playback resumed successfully');
            } catch (playError) {
                console.warn('Could not resume playback:', playError.message);

                // Try one more time with user gesture simulation
                try {
                    // Simulate a click on the video element (user gesture)
                    videoElement.dispatchEvent(new MouseEvent('click', {
                        bubbles: true,
                        cancelable: true,
                        view: window
                    }));

                    await new Promise(resolve => setTimeout(resolve, 100));
                    await videoElement.play();
                    console.log('Playback resumed after simulated click');
                } catch (secondError) {
                    console.error('Second resume attempt failed:', secondError);
                }
            }
        }

        console.log(`=== SET VIDEO TIME COMPLETE: Target=${time}s, Actual=${actualTime}s ===`);
        return Math.abs(actualTime - time) <= 0.5;

    } catch (error) {
        console.error('=== SET VIDEO TIME ERROR:', error);
        return false;
    }
};

// Check if time is within seekable range
window.isTimeSeekable = function (videoElement, time) {
    try {
        if (!videoElement || videoElement.seekable.length === 0) {
            return false;
        }

        const seekableStart = videoElement.seekable.start(0);
        const seekableEnd = videoElement.seekable.end(0);

        return time >= seekableStart && time <= seekableEnd;
    } catch (error) {
        console.error('Error checking seekable range:', error);
        return false;
    }
};

// Get seekable range
window.getSeekableRange = function (videoElement) {
    try {
        if (!videoElement || videoElement.seekable.length === 0) {
            return { start: 0, end: 0 };
        }

        return {
            start: videoElement.seekable.start(0),
            end: videoElement.seekable.end(0)
        };
    } catch (error) {
        console.error('Error getting seekable range:', error);
        return { start: 0, end: 0 };
    }
};

// Wait for video to be seekable
window.waitForSeekable = async function (videoElement, timeout = 5000) {
    try {
        console.log('Waiting for video to be seekable...');

        // Check if already seekable
        if (videoElement.seekable.length > 0 && videoElement.seekable.end(0) > 0) {
            console.log('Video already seekable');
            return true;
        }

        return new Promise((resolve, reject) => {
            let timeoutId;

            const checkSeekable = () => {
                if (videoElement.seekable.length > 0 && videoElement.seekable.end(0) > 0) {
                    console.log('Video now seekable, range:',
                        videoElement.seekable.start(0), '-',
                        videoElement.seekable.end(0));
                    clearTimeout(timeoutId);
                    videoElement.removeEventListener('progress', checkSeekable);
                    videoElement.removeEventListener('loadedmetadata', checkSeekable);
                    resolve(true);
                }
            };

            // Check periodically
            const intervalId = setInterval(checkSeekable, 100);

            // Also listen for progress events
            videoElement.addEventListener('progress', checkSeekable);
            videoElement.addEventListener('loadedmetadata', checkSeekable);

            // Timeout
            timeoutId = setTimeout(() => {
                clearInterval(intervalId);
                videoElement.removeEventListener('progress', checkSeekable);
                videoElement.removeEventListener('loadedmetadata', checkSeekable);
                console.warn('Timeout waiting for video to be seekable');
                resolve(false);
            }, timeout);
        });
    } catch (error) {
        console.error('Error in waitForSeekable:', error);
        return false;
    }
};
// Enhanced video play with better error handling
window.playVideo = async function (videoElement) {
    try {
        if (!videoElement || !videoElement.play) {
            console.warn('Video element or play method not available');
            return false;
        }

        // Check if already playing
        if (!videoElement.paused) {
            console.log('Video is already playing');
            return true;
        }

        // Clear any pending operations
        if (window.videoPlayPromise) {
            try {
                window.videoPlayPromise.catch(() => { });
            } catch (e) {
                // Ignore
            }
            window.videoPlayPromise = null;
        }

        // Play with error handling
        window.videoPlayPromise = videoElement.play();
        await window.videoPlayPromise;

        console.log('Video play successful');
        return true;

    } catch (error) {
        console.error('Error playing video:', error);

        // Handle specific errors
        if (error.name === 'AbortError') {
            console.log('Play was aborted (likely by pause)');
        } else if (error.name === 'NotAllowedError') {
            console.warn('Autoplay not allowed - user interaction required');
        } else if (error.name === 'NotSupportedError') {
            console.warn('Video format not supported');
        }

        return false;
    }
};

// Enhanced getVideoCurrentTime with fallback
window.getVideoCurrentTime = function (videoElement) {
    try {
        if (videoElement && !isNaN(videoElement.currentTime)) {
            return videoElement.currentTime;
        }
        return 0;
    } catch (error) {
        console.error('Error getting video currentTime:', error);
        return 0;
    }
};

// Force video to specific time (more aggressive)
window.forceVideoTime = async function (videoElement, time) {
    try {
        console.log(`Forcing video time to: ${time}s`);

        // Pause first if playing
        const wasPlaying = !videoElement.paused;
        if (wasPlaying) {
            videoElement.pause();
            await new Promise(resolve => setTimeout(resolve, 100));
        }

        // Set time directly
        videoElement.currentTime = time;

        // Force update by triggering events
        videoElement.dispatchEvent(new Event('timeupdate'));

        // Wait a moment
        await new Promise(resolve => setTimeout(resolve, 200));

        // Check result
        const actualTime = videoElement.currentTime;
        console.log(`Actual time after force: ${actualTime}s`);

        // Resume if was playing
        if (wasPlaying) {
            await new Promise(resolve => setTimeout(resolve, 100));
            try {
                await videoElement.play();
            } catch (e) {
                console.warn('Could not resume after force seek:', e);
            }
        }

        return actualTime;

    } catch (error) {
        console.error('Error forcing video time:', error);
        return 0;
    }
};

window.getVideoDuration = function (videoElement) {
    try {
        if (videoElement && !isNaN(videoElement.duration) && videoElement.duration > 0) {
            return videoElement.duration;
        }
        return 0;
    } catch (error) {
        console.error('Error getting video duration:', error);
        return 0;
    }
};

// Simple video player initialization (no event handler attachment)
window.initializeVideoPlayer = function (videoElement) {
    try {
        if (!videoElement) return false;
        console.log('Video element ready for playback');
        return true;
    } catch (error) {
        console.error('Error with video element:', error);
        return false;
    }
};

// ==================== AUDIO PLAYER FUNCTIONS ====================
// ==================== AUDIO PLAYER FUNCTIONS (ENHANCED) ====================
// ==================== ENHANCED AUDIO FUNCTIONS ====================

// Set audio playback rate (speed)
window.setAudioPlaybackRate = function (audioElement, rate) {
    try {
        if (audioElement) {
            audioElement.playbackRate = rate;
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error setting playback rate:', error);
        return false;
    }
};

// Set audio loop
window.setAudioLoop = function (audioElement, loop) {
    try {
        if (audioElement) {
            audioElement.loop = loop;
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error setting audio loop:', error);
        return false;
    }
};

// Get audio buffered ranges
window.getAudioBufferedRanges = function (audioElement) {
    try {
        if (!audioElement || audioElement.buffered.length === 0) {
            return [];
        }

        const ranges = [];
        for (let i = 0; i < audioElement.buffered.length; i++) {
            ranges.push({
                start: audioElement.buffered.start(i),
                end: audioElement.buffered.end(i)
            });
        }
        return ranges;
    } catch (error) {
        console.error('Error getting buffered ranges:', error);
        return [];
    }
};

// Initialize audio visualization (basic waveform)
window.initializeAudioVisualization = function (canvasId, audioElement) {
    try {
        const canvas = document.getElementById(canvasId);
        if (!canvas || !audioElement) return false;

        const ctx = canvas.getContext('2d');
        const width = canvas.width;
        const height = canvas.height;

        // Clear canvas
        ctx.clearRect(0, 0, width, height);

        // Draw background
        ctx.fillStyle = 'rgba(0, 0, 0, 0.2)';
        ctx.fillRect(0, 0, width, height);

        // Draw progress indicator
        const progress = audioElement.currentTime / audioElement.duration;
        const progressX = width * progress;

        ctx.fillStyle = 'rgba(0, 123, 255, 0.3)';
        ctx.fillRect(0, 0, progressX, height);

        // Draw waveform (simplified - would need AudioContext for real visualization)
        ctx.strokeStyle = '#007bff';
        ctx.lineWidth = 2;
        ctx.beginPath();

        const centerY = height / 2;
        const segmentWidth = 4;
        const segments = Math.floor(width / segmentWidth);

        for (let i = 0; i < segments; i++) {
            const x = i * segmentWidth;
            const progressAtX = x / width;

            // Simulated amplitude (in a real app, you'd analyze the audio)
            const amplitude = Math.sin(progressAtX * Math.PI * 20) *
                (1 - Math.abs(progress - progressAtX)) *
                (height * 0.4);

            const y1 = centerY - amplitude;
            const y2 = centerY + amplitude;

            ctx.moveTo(x, y1);
            ctx.lineTo(x, y2);
        }

        ctx.stroke();

        return true;
    } catch (error) {
        console.error('Error initializing audio visualization:', error);
        return false;
    }
};

// Update audio visualization
window.updateAudioVisualization = function (canvasId, audioElement) {
    try {
        if (!audioElement || audioElement.paused) return false;

        return window.initializeAudioVisualization(canvasId, audioElement);
    } catch (error) {
        console.error('Error updating audio visualization:', error);
        return false;
    }
};
// Get audio source
window.getAudioSrc = function (audioElement) {
    try {
        if (audioElement) {
            return audioElement.src;
        }
        return '';
    } catch (error) {
        console.error('Error getting audio src:', error);
        return '';
    }
};

// Set audio source
window.setAudioSrc = function (audioElement, src) {
    try {
        if (audioElement) {
            audioElement.src = src;
            audioElement.load(); // Force reload
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error setting audio src:', error);
        return false;
    }
};

// Enhanced setAudioCurrentTime
window.setAudioCurrentTime = async function (audioElement, time) {
    try {
        if (!audioElement || isNaN(time)) {
            console.error('Invalid audio element or time');
            return false;
        }

        console.log(`=== SET AUDIO TIME: ${time}s ===`);
        console.log('Audio state:', {
            duration: audioElement.duration,
            currentTime: audioElement.currentTime,
            paused: audioElement.paused,
            readyState: audioElement.readyState,
            networkState: audioElement.networkState,
            buffered: audioElement.buffered.length > 0 ?
                `${audioElement.buffered.start(0)}-${audioElement.buffered.end(0)}` : 'empty',
            seekable: audioElement.seekable.length > 0 ?
                `${audioElement.seekable.start(0)}-${audioElement.seekable.end(0)}` : 'empty'
        });

        // Validate time is within seekable range
        if (audioElement.seekable.length > 0) {
            const seekableStart = audioElement.seekable.start(0);
            const seekableEnd = audioElement.seekable.end(0);

            if (time < seekableStart || time > seekableEnd) {
                console.warn(`Time ${time}s is outside seekable range ${seekableStart}-${seekableEnd}s`);
                time = Math.max(seekableStart, Math.min(seekableEnd, time));
                console.log(`Adjusted time to: ${time}s`);
            }
        }

        // Check if audio is ready for seeking
        if (audioElement.readyState < 2) { // HAVE_CURRENT_DATA or higher
            console.warn('Audio not ready for seeking (readyState:', audioElement.readyState, ')');

            // Wait for audio to load more data
            console.log('Waiting for audio to load...');
            await new Promise((resolve, reject) => {
                const onLoadedData = () => {
                    audioElement.removeEventListener('loadeddata', onLoadedData);
                    audioElement.removeEventListener('error', onError);
                    console.log('Audio data loaded, readyState:', audioElement.readyState);
                    resolve();
                };

                const onError = () => {
                    audioElement.removeEventListener('loadeddata', onLoadedData);
                    audioElement.removeEventListener('error', onError);
                    reject(new Error('Audio load error'));
                };

                audioElement.addEventListener('loadeddata', onLoadedData, { once: true });
                audioElement.addEventListener('error', onError, { once: true });

                // Timeout after 3 seconds
                setTimeout(() => {
                    audioElement.removeEventListener('loadeddata', onLoadedData);
                    audioElement.removeEventListener('error', onError);
                    console.log('Audio load timeout, proceeding anyway');
                    resolve();
                }, 3000);
            });
        }

        // Store playing state
        const wasPlaying = !audioElement.paused;
        console.log('Was playing:', wasPlaying);

        // Pause if playing
        if (wasPlaying) {
            console.log('Pausing audio...');
            audioElement.pause();
            await new Promise(resolve => setTimeout(resolve, 50));
        }

        // Method 1: Direct assignment
        console.log(`Setting currentTime to ${time}s...`);
        audioElement.currentTime = time;

        // Wait for time to update
        await new Promise(resolve => setTimeout(resolve, 100));

        // Check result
        let actualTime = audioElement.currentTime;
        console.log(`Actual time after setting: ${actualTime}s`);

        // If time wasn't set correctly, try alternative methods
        if (Math.abs(actualTime - time) > 0.5) {
            console.warn(`Time mismatch! Expected: ${time}s, Got: ${actualTime}s`);

            // Method 2: For streaming audio, try to reload with time parameter
            if (time > 0) {
                console.log('Trying to force seek via src change...');
                try {
                    // Store current src
                    const currentSrc = audioElement.src;
                    const currentType = audioElement.getAttribute('type');

                    // Temporarily change src to force seeking (for streaming)
                    audioElement.src = currentSrc + (currentSrc.includes('?') ? '&' : '?') +
                        't=' + Math.floor(time) + '&_' + Date.now();

                    if (currentType) {
                        audioElement.setAttribute('type', currentType);
                    }

                    await new Promise(resolve => {
                        const onCanPlay = () => {
                            audioElement.removeEventListener('canplay', onCanPlay);
                            resolve();
                        };
                        audioElement.addEventListener('canplay', onCanPlay, { once: true });

                        // Load the audio
                        audioElement.load();
                    });

                    // Set time again
                    audioElement.currentTime = time;
                    await new Promise(resolve => setTimeout(resolve, 300));

                    actualTime = audioElement.currentTime;
                    console.log(`Actual time after src change: ${actualTime}s`);

                    // Restore original src if still not correct
                    if (Math.abs(actualTime - time) > 0.5) {
                        console.log('Restoring original src...');
                        audioElement.src = currentSrc;
                        if (currentType) {
                            audioElement.setAttribute('type', currentType);
                        }
                        audioElement.load();
                        await new Promise(resolve => setTimeout(resolve, 200));
                        audioElement.currentTime = time;
                        await new Promise(resolve => setTimeout(resolve, 200));
                        actualTime = audioElement.currentTime;
                        console.log(`Actual time after restore: ${actualTime}s`);
                    }

                } catch (srcError) {
                    console.error('Error with src change method:', srcError);
                }
            }
        }

        // Resume if was playing
        if (wasPlaying && actualTime >= 0) {
            console.log('Attempting to resume playback...');
            await new Promise(resolve => setTimeout(resolve, 200));

            try {
                await audioElement.play();
                console.log('Playback resumed successfully');
            } catch (playError) {
                console.warn('Could not resume playback:', playError.message);

                // Try one more time with user gesture simulation
                try {
                    // Simulate a click on the audio element (user gesture)
                    audioElement.dispatchEvent(new MouseEvent('click', {
                        bubbles: true,
                        cancelable: true,
                        view: window
                    }));

                    await new Promise(resolve => setTimeout(resolve, 100));
                    await audioElement.play();
                    console.log('Playback resumed after simulated click');
                } catch (secondError) {
                    console.error('Second resume attempt failed:', secondError);
                }
            }
        }

        console.log(`=== SET AUDIO TIME COMPLETE: Target=${time}s, Actual=${actualTime}s ===`);
        return Math.abs(actualTime - time) <= 0.5;

    } catch (error) {
        console.error('=== SET AUDIO TIME ERROR:', error);
        return false;
    }
};

// Check if audio time is seekable
window.isAudioTimeSeekable = function (audioElement, time) {
    try {
        if (!audioElement || audioElement.seekable.length === 0) {
            return false;
        }

        const seekableStart = audioElement.seekable.start(0);
        const seekableEnd = audioElement.seekable.end(0);

        return time >= seekableStart && time <= seekableEnd;
    } catch (error) {
        console.error('Error checking audio seekable range:', error);
        return false;
    }
};

// Get audio seekable range
window.getAudioSeekableRange = function (audioElement) {
    try {
        if (!audioElement || audioElement.seekable.length === 0) {
            return { start: 0, end: 0 };
        }

        return {
            start: audioElement.seekable.start(0),
            end: audioElement.seekable.end(0)
        };
    } catch (error) {
        console.error('Error getting audio seekable range:', error);
        return { start: 0, end: 0 };
    }
};
// Simple audio time setter (works for non-streaming audio)
window.setAudioCurrentTimeSimple = function (audioElement, time) {
    try {
        if (audioElement && !isNaN(time)) {
            audioElement.currentTime = time;
            console.log(`Audio time set to ${time}s (simple method)`);
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error in setAudioCurrentTimeSimple:', error);
        return false;
    }
};

// Get audio duration
window.getAudioDuration = function (audioElement) {
    try {
        if (audioElement && !isNaN(audioElement.duration)) {
            return audioElement.duration;
        }
        return 0;
    } catch (error) {
        console.error('Error getting audio duration:', error);
        return 0;
    }
};

// Check if audio is playing
window.isAudioPlaying = function (audioElement) {
    try {
        if (audioElement) {
            return !audioElement.paused && !audioElement.ended;
        }
        return false;
    } catch (error) {
        console.error('Error checking if audio is playing:', error);
        return false;
    }
};

// Replace audio element completely
window.replaceAudioElement = function (audioId, newSrc, startTime) {
    try {
        console.log(`Replacing audio element ${audioId} with src: ${newSrc}, start time: ${startTime}`);

        const oldAudio = document.getElementById(audioId);
        if (!oldAudio) {
            console.error(`Audio element with id ${audioId} not found`);
            return false;
        }

        // Store old state
        const wasPlaying = !oldAudio.paused;
        const oldVolume = oldAudio.volume;
        const oldMuted = oldAudio.muted;

        // Create new audio element
        const newAudio = document.createElement('audio');
        newAudio.id = audioId;
        newAudio.src = newSrc;
        newAudio.preload = 'auto';

        // Copy attributes
        if (oldAudio.hasAttribute('controls')) newAudio.controls = true;
        if (oldAudio.hasAttribute('autoplay')) newAudio.autoplay = true;
        if (oldAudio.hasAttribute('loop')) newAudio.loop = true;

        // Set time once loaded
        newAudio.onloadeddata = function () {
            console.log('New audio loaded, setting time to:', startTime);
            newAudio.currentTime = startTime;

            // Restore volume
            newAudio.volume = oldVolume;
            newAudio.muted = oldMuted;

            // Play if was playing
            if (wasPlaying) {
                setTimeout(() => {
                    newAudio.play().catch(e => {
                        console.log('Could not auto-play after replace:', e);
                    });
                }, 100);
            }
        };

        // Handle errors
        newAudio.onerror = function (e) {
            console.error('New audio element error:', e);
        };

        // Replace in DOM
        oldAudio.parentNode.replaceChild(newAudio, oldAudio);

        console.log('Audio element replaced successfully');
        return true;

    } catch (error) {
        console.error('Error replacing audio element:', error);
        return false;
    }
};

window.ensureDropdownPosition = function () {
    // This function ensures dropdowns stay within viewport
    document.addEventListener('shown.bs.dropdown', function (event) {
        var dropdown = event.target.nextElementSibling;
        if (dropdown && dropdown.classList.contains('dropdown-menu')) {
            var rect = dropdown.getBoundingClientRect();
            var viewportWidth = window.innerWidth;

            // If dropdown goes beyond viewport, adjust it
            if (rect.right > viewportWidth) {
                dropdown.style.left = 'auto';
                dropdown.style.right = '0';
                dropdown.style.transform = 'translateX(0)';
            }

            // If dropdown goes off the left side
            if (rect.left < 0) {
                dropdown.style.left = '0';
                dropdown.style.right = 'auto';
                dropdown.style.transform = 'translateX(0)';
            }
        }
    });
};

// Debug function to check audio element state
window.debugAudioElement = function (audioElement) {
    try {
        if (!audioElement) {
            return { error: 'No audio element' };
        }

        return {
            src: audioElement.src,
            currentTime: audioElement.currentTime,
            duration: audioElement.duration,
            paused: audioElement.paused,
            ended: audioElement.ended,
            readyState: audioElement.readyState,
            networkState: audioElement.networkState,
            buffered: audioElement.buffered.length > 0 ?
                `Start: ${audioElement.buffered.start(0)}, End: ${audioElement.buffered.end(0)}` : 'Empty',
            seekable: audioElement.seekable.length > 0 ?
                `Start: ${audioElement.seekable.start(0)}, End: ${audioElement.seekable.end(0)}` : 'Empty',
            volume: audioElement.volume,
            muted: audioElement.muted
        };
    } catch (error) {
        return { error: error.message };
    }
};
// Wait for audio to be seekable
window.waitForAudioSeekable = async function (audioElement, timeout = 5000) {
    try {
        console.log('Waiting for audio to be seekable...');

        // Check if already seekable
        if (audioElement.seekable.length > 0 && audioElement.seekable.end(0) > 0) {
            console.log('Audio already seekable');
            return true;
        }

        return new Promise((resolve, reject) => {
            let timeoutId;

            const checkSeekable = () => {
                if (audioElement.seekable.length > 0 && audioElement.seekable.end(0) > 0) {
                    console.log('Audio now seekable, range:',
                        audioElement.seekable.start(0), '-',
                        audioElement.seekable.end(0));
                    clearTimeout(timeoutId);
                    audioElement.removeEventListener('progress', checkSeekable);
                    audioElement.removeEventListener('loadedmetadata', checkSeekable);
                    resolve(true);
                }
            };

            // Check periodically
            const intervalId = setInterval(checkSeekable, 100);

            // Also listen for progress events
            audioElement.addEventListener('progress', checkSeekable);
            audioElement.addEventListener('loadedmetadata', checkSeekable);

            // Timeout
            timeoutId = setTimeout(() => {
                clearInterval(intervalId);
                audioElement.removeEventListener('progress', checkSeekable);
                audioElement.removeEventListener('loadedmetadata', checkSeekable);
                console.warn('Timeout waiting for audio to be seekable');
                resolve(false);
            }, timeout);
        });
    } catch (error) {
        console.error('Error in waitForAudioSeekable:', error);
        return false;
    }
};

// Force audio to specific time
window.forceAudioTime = async function (audioElement, time) {
    try {
        console.log(`Forcing audio time to: ${time}s`);

        // Pause first if playing
        const wasPlaying = !audioElement.paused;
        if (wasPlaying) {
            audioElement.pause();
            await new Promise(resolve => setTimeout(resolve, 100));
        }

        // Set time directly
        audioElement.currentTime = time;

        // Force update by triggering events
        audioElement.dispatchEvent(new Event('timeupdate'));

        // Wait a moment
        await new Promise(resolve => setTimeout(resolve, 200));

        // Check result
        const actualTime = audioElement.currentTime;
        console.log(`Actual time after force: ${actualTime}s`);

        // Resume if was playing
        if (wasPlaying) {
            await new Promise(resolve => setTimeout(resolve, 100));
            try {
                await audioElement.play();
            } catch (e) {
                console.warn('Could not resume after force seek:', e);
            }
        }

        return actualTime;

    } catch (error) {
        console.error('Error forcing audio time:', error);
        return 0;
    }
};
window.playAudio = async (audioElement) => {
    try {
        await audioElement.play();
        return true;
    } catch (error) {
        console.error('Error playing audio:', error);
        return false;
    }
};

window.pauseAudio = (audioElement) => {
    try {
        audioElement.pause();
        return true;
    } catch (error) {
        console.error('Error pausing audio:', error);
        return false;
    }
};

window.setAudioMuted = (audioElement, muted) => {
    try {
        audioElement.muted = muted;
        return true;
    } catch (error) {
        console.error('Error setting audio muted:', error);
        return false;
    }
};

window.setElementVolume = (element, volume) => {
    try {
        if (element) {
            element.volume = volume;
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error setting volume:', error);
        return false;
    }
};

window.getAudioCurrentTime = (audioElement) => {
    try {
        if (audioElement && !isNaN(audioElement.currentTime)) {
            return audioElement.currentTime;
        }
        return 0;
    } catch (error) {
        console.error('Error getting audio currentTime:', error);
        return 0;
    }
};


// Check if video is paused
window.isVideoPaused = function (videoElement) {
    try {
        if (videoElement) {
            return videoElement.paused;
        }
        return true;
    } catch (error) {
        console.error('Error checking video pause state:', error);
        return true;
    }
};
// ==================== DOCUMENT CONVERSION FUNCTIONS ====================

// Mammoth.js document conversion
async function convertDocxToHtml(base64String) {
    try {
        console.log('Starting document conversion...');

        // Ensure Mammoth is loaded
        if (typeof window.mammoth === 'undefined') {
            console.log('Loading Mammoth.js...');
            await loadMammothLibrary();
        }

        // Convert base64 to array buffer
        const binaryString = atob(base64String);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        const arrayBuffer = bytes.buffer;

        console.log('Converting DOCX to HTML with Mammoth...');

        // Use Mammoth with better options
        const result = await window.mammoth.convertToHtml({
            arrayBuffer: arrayBuffer
        }, {
            styleMap: [
                "p[style-name='Heading 1'] => h1:fresh",
                "p[style-name='Heading 2'] => h2:fresh",
                "p[style-name='Heading 3'] => h3:fresh",
                "p[style-name='Heading 4'] => h4:fresh",
                "p[style-name='Heading 5'] => h5:fresh",
                "p[style-name='Heading 6'] => h6:fresh",
                "r[style-name='Strong'] => strong",
                "p[style-name='List Paragraph'] => ul > li:fresh",
                "p[style-name='Quote'] => blockquote > p:fresh"
            ],
            includeDefaultStyleMap: true
        });

        // Extract messages
        let messages = '';
        if (result.messages && result.messages.length > 0) {
            messages = result.messages.map(msg => {
                if (typeof msg === 'string') return msg;
                if (msg.message) return msg.message;
                if (msg.type) return `${msg.type}: ${msg.message || JSON.stringify(msg)}`;
                return JSON.stringify(msg);
            }).join('\n');
            console.log('Conversion messages:', messages);
        }

        if (result.value) {
            console.log('Conversion successful, HTML generated');
            return {
                html: result.value,
                error: messages || null
            };
        } else {
            console.log('No HTML generated, trying text extraction...');
            // Fallback to text extraction
            return await extractDocxText(base64String);
        }

    } catch (error) {
        console.error('Error in convertDocxToHtml:', error);
        return {
            html: null,
            error: error.message || 'Unknown conversion error'
        };
    }
}

// Text extraction fallback
async function extractDocxText(base64String) {
    try {
        // Convert base64 to array buffer
        const binaryString = atob(base64String);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        const arrayBuffer = bytes.buffer;

        // Extract raw text
        const result = await window.mammoth.extractRawText({ arrayBuffer: arrayBuffer });

        // Format text as HTML
        const text = result.value || '';
        const html = `
            <div style="white-space: pre-wrap; font-family: inherit; line-height: 1.6;">
                ${text.replace(/\n/g, '<br>').replace(/\t/g, '&nbsp;&nbsp;&nbsp;&nbsp;')}
            </div>
        `;

        return {
            html: html,
            error: 'Document displayed as text-only',
            isTextOnly: true
        };
    } catch (error) {
        console.error('Error extracting text:', error);
        return {
            html: null,
            error: error.message
        };
    }
}

// Load Mammoth.js library dynamically
async function loadMammothLibrary() {
    return new Promise((resolve, reject) => {
        if (window.mammoth) {
            console.log('Mammoth.js already loaded');
            resolve();
            return;
        }

        console.log('Loading Mammoth.js...');
        const script = document.createElement('script');
        script.src = 'https://unpkg.com/mammoth@1.5.1/mammoth.browser.min.js';
        script.crossOrigin = 'anonymous';

        script.onload = () => {
            console.log('Mammoth.js loaded successfully');
            resolve();
        };

        script.onerror = (error) => {
            console.error('Failed to load Mammoth.js:', error);
            reject(new Error('Failed to load Mammoth.js library'));
        };

        document.head.appendChild(script);
    });
}

// Check if Mammoth.js is loaded
window.checkMammothLoaded = function () {
    return typeof window.mammoth !== 'undefined';
};

// Copy document text from HTML
window.copyDocumentText = function (html) {
    try {
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = html;
        const text = tempDiv.textContent || tempDiv.innerText || '';

        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        document.body.appendChild(textarea);
        textarea.select();

        const success = document.execCommand('copy');
        document.body.removeChild(textarea);

        return success;
    } catch (error) {
        console.error('Error copying text:', error);
        return false;
    }
};

// ==================== UTILITY FUNCTIONS ====================

// Copy text to clipboard
window.copyToClipboard = async (text) => {
    try {
        if (!text || text.length === 0) {
            throw new Error('No text to copy');
        }

        await navigator.clipboard.writeText(text);
        console.log('Text copied to clipboard');
        return true;

    } catch (error) {
        console.error('Error copying to clipboard:', error);

        // Fallback for older browsers
        try {
            const textArea = document.createElement('textarea');
            textArea.value = text;
            textArea.style.position = 'fixed';
            textArea.style.left = '-999999px';
            textArea.style.top = '-999999px';
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
            console.log('Text copied to clipboard (fallback)');
            return true;
        } catch (fallbackError) {
            console.error('Fallback copy also failed:', fallbackError);
            return false;
        }
    }
};

// Fullscreen functions
window.toggleFullScreen = (element) => {
    try {
        if (!document.fullscreenElement) {
            // Enter fullscreen
            if (element.requestFullscreen) {
                return element.requestFullscreen();
            } else if (element.webkitRequestFullscreen) {
                return element.webkitRequestFullscreen();
            } else if (element.msRequestFullscreen) {
                return element.msRequestFullscreen();
            }
        } else {
            // Exit fullscreen
            if (document.exitFullscreen) {
                return document.exitFullscreen();
            } else if (document.webkitExitFullscreen) {
                return document.webkitExitFullscreen();
            } else if (document.msExitFullscreen) {
                return document.msExitFullscreen();
            }
        }
        return Promise.resolve();
    } catch (error) {
        console.error('Error toggling fullscreen:', error);
        return Promise.reject(error);
    }
};

// Load script dynamically
window.loadScript = (src) => {
    return new Promise((resolve, reject) => {
        // Check if script is already loaded
        const existingScript = document.querySelector(`script[src="${src}"]`);
        if (existingScript) {
            resolve();
            return;
        }

        const script = document.createElement('script');
        script.src = src;
        script.type = 'text/javascript';
        script.async = true;

        script.onload = () => {
            console.log(`Script loaded: ${src}`);
            resolve();
        };

        script.onerror = (error) => {
            console.error(`Failed to load script: ${src}`, error);
            reject(error);
        };

        document.head.appendChild(script);
    });
};

// Progress tooltip functions
window.showProgressTooltip = function (tooltipElement, x, y) {
    try {
        if (tooltipElement && tooltipElement.style) {
            tooltipElement.style.left = (x - 30) + 'px';
            tooltipElement.style.top = (y - 40) + 'px';
            tooltipElement.style.display = 'block';
        }
    } catch (error) {
        console.error('Error showing progress tooltip:', error);
    }
};

window.hideProgressTooltip = function (tooltipElement) {
    try {
        if (tooltipElement && tooltipElement.style) {
            tooltipElement.style.display = 'none';
        }
    } catch (error) {
        console.error('Error hiding progress tooltip:', error);
    }
};

// Get element at point
window.getElementFromPoint = function (x, y) {
    return document.elementFromPoint(x, y);
};

// Get element class name
window.getElementClassName = function (element) {
    return element ? (element.className || '') : '';
};

// Get bounding client rect
window.getBoundingClientRect = function (element) {
    try {
        if (element) {
            return element.getBoundingClientRect();
        }
        return null;
    } catch (error) {
        console.error('Error getting bounding rect:', error);
        return null;
    }
};

// Show toast notification
window.showToast = (type, message) => {
    try {
        console.log(`Toast [${type}]: ${message}`);

        // Create toast container if it doesn't exist
        let toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toast-container';
            toastContainer.style.position = 'fixed';
            toastContainer.style.top = '20px';
            toastContainer.style.right = '20px';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }

        // Create toast element
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.style.cssText = `
            background: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#ffc107'};
            color: white;
            padding: 12px 20px;
            margin-bottom: 10px;
            border-radius: 4px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            animation: slideIn 0.3s ease, fadeOut 0.3s ease 2.7s;
            min-width: 250px;
            max-width: 400px;
            word-wrap: break-word;
        `;

        // Add icon based on type
        const icon = type === 'success' ? '?' : type === 'error' ? '?' : '?';
        toast.innerHTML = `
            <span style="margin-right: 10px; font-weight: bold;">${icon}</span>
            ${message}
        `;

        // Add CSS animations if not already added
        if (!document.querySelector('#toast-animations')) {
            const style = document.createElement('style');
            style.id = 'toast-animations';
            style.textContent = `
                @keyframes slideIn {
                    from { transform: translateX(100%); opacity: 0; }
                    to { transform: translateX(0); opacity: 1; }
                }
                @keyframes fadeOut {
                    from { opacity: 1; }
                    to { opacity: 0; }
                }
            `;
            document.head.appendChild(style);
        }

        toastContainer.appendChild(toast);

        // Remove toast after 3 seconds
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 3000);

        return true;

    } catch (error) {
        console.error('Error showing toast:', error);
        return false;
    }
};

// Fallback PDF rendering using iframe
window.renderPdfFallback = (containerId, dataUrl, title) => {
    try {
        console.log('Using fallback PDF render with iframe');

        const container = document.getElementById(containerId);
        if (!container) {
            console.error(`Container with ID ${containerId} not found`);
            throw new Error(`Container with ID ${containerId} not found`);
        }

        // Clear container
        container.innerHTML = '';

        // Create iframe
        const iframe = document.createElement('iframe');
        iframe.src = dataUrl;
        iframe.title = title;
        iframe.style.cssText = `
            width: 100%;
            height: 100%;
            border: none;
            display: block;
        `;

        // Set sandbox to restrict capabilities
        iframe.sandbox = "allow-scripts allow-same-origin";

        container.appendChild(iframe);
        console.log('Fallback PDF iframe created');
        return true;

    } catch (error) {
        console.error('Error in fallback PDF render:', error);
        return false;
    }
};

// Download file from base64 data
window.downloadFile = (base64Data, fileName, mimeType = 'application/octet-stream') => {
    try {
        console.log(`Downloading file: ${fileName}, Type: ${mimeType}`);

        // Convert base64 to blob
        const binaryString = atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        const blob = new Blob([bytes], { type: mimeType });

        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.style.display = 'none';

        // Trigger download
        document.body.appendChild(link);
        link.click();

        // Cleanup
        setTimeout(() => {
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);
        }, 100);

        console.log('Download initiated');
        return true;

    } catch (error) {
        console.error('Error downloading file:', error);
        return false;
    }
};

// ==================== EXPORT FUNCTIONS ====================

// Export functions for Blazor
window.convertDocxToHtml = convertDocxToHtml;
window.loadMammothLibrary = loadMammothLibrary;

// ==================== INITIALIZATION ====================

// Initialize when page loads
document.addEventListener('DOMContentLoaded', () => {
    console.log('Secure File Viewer JS loaded (Fixed Version)');

    // Initialize PDF.js if available
    if (typeof pdfjsLib !== 'undefined') {
        // Set the worker source
        pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
        console.log('PDF.js worker configured');

        // You can also set version compatibility
        pdfjsLib.version = '3.11.174';
    }
});

// Helper function for DOM ready state
window.domReady = (callback) => {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', callback);
    } else {
        callback();
    }
};

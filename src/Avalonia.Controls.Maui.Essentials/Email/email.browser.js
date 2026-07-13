export const emailInterop =
{
    openEml: function (content) {
        const blob = new Blob([content], { type: 'message/rfc822' });
        const url = URL.createObjectURL(blob);
        window.location.href = url;
        // Clean up
        setTimeout(() => URL.revokeObjectURL(url), 10_000);
    }
};
export function base64UrlEncodeString(input: string): string {
    const utf8Encode = new TextEncoder().encode(input);
    let binary = '';
    utf8Encode.forEach(b => (binary += String.fromCharCode(b)));

    return btoa(binary)
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=+$/, '');
}

export function base64UrlDecodeString(input: string): string {
    const base64 = input
        .replace(/-/g, '+')
        .replace(/_/g, '/')
        .padEnd(input.length + (4 - (input.length % 4)) % 4, '=');

    const binary = atob(base64);
    const bytes = Uint8Array.from(binary, c => c.charCodeAt(0));

    return new TextDecoder().decode(bytes);
}
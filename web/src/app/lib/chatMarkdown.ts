import markdownit from "markdown-it"

const markdown = markdownit({
    breaks: true,
})

export const renderChatMarkdown = (value: string) => {
    const source = value.trim()

    if (!source) {
        return ""
    }

    return markdown.render(source)
}
const LANG_KEY_RE = /^[a-z]{2,3}(-[A-Za-z]{2,4})?$/
const SUPPORTED_LANGS = new Set(['en', 'ru'])

function isTranslatable(obj) {
  const keys = Object.keys(obj)
  return keys.length > 0 && keys.every(k => LANG_KEY_RE.test(k))
}

function localizeNode(node, lang) {
  if (Array.isArray(node))
    return node.map(item => localizeNode(item, lang))

  if (node !== null && typeof node === 'object') {
    if (isTranslatable(node))
      return node[lang] ?? node['en'] ?? Object.values(node)[0]

    const result = {}
    for (const [key, value] of Object.entries(node))
      result[key] = localizeNode(value, lang)
    return result
  }

  return node
}

function warnIfUnsupportedLang(lang, endpoint) {
  if (!SUPPORTED_LANGS.has(lang))
    console.warn(`[localize] Unknown lang="${lang}" requested at ${endpoint}, falling back to "en"`)
}

module.exports = { localizeNode, warnIfUnsupportedLang }
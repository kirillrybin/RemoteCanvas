const { localizeNode, warnIfUnsupportedLang} = require('./localize')
  // GET /pages/:name — A/B group resolution + lang localization
module.exports = function registerPagesRoute(server, router) {
  server.get('/pages/:name', (req, res) => {
    const name   = req.params.name.replace('.json', '')
    const userId = req.query.userId ?? 'default'
    const lang   = req.query.lang   ?? 'en'
    warnIfUnsupportedLang(lang, req.path)
    
    const db = router.db.getState()

    const group  = userId.charCodeAt(userId.length - 1) % 2 === 0 ? 'b' : 'a'
    const abName = `${name}_${group}`

    const page = db.pages.find(p => p.id === abName)
      ?? db.pages.find(p => p.id === name)

    if (!page)
      return res.status(404).json({ error: `Page '${name}' not found` })

    res.json({ ...localizeNode(page, lang), _abGroup: page.id === abName ? group : 'default' })
  })
}
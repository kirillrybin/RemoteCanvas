const { localizeNode, warnIfUnsupportedLang } = require('./localize')

// GET /news — returns all news sorted: pinned first, then by date desc
module.exports = function registerNewsRoutes(server, router) {

  server.get('/news', (req, res) => {
    const db = router.db.getState()
    const limit = parseInt(req.query.limit) || 10
    const lang = req.query.lang ?? 'en'
    warnIfUnsupportedLang(lang, req.path)

    const sorted = [...db.news]
      .sort((a, b) => {
        if (a.pinned !== b.pinned) return a.pinned ? -1 : 1
        return new Date(b.publishedAt) - new Date(a.publishedAt)
      })
      .slice(0, limit)
      .map(item => localizeNode(item, lang))

    res.json(sorted)
  })

  server.get('/news/:id', (req, res) => {
    const lang = req.query.lang ?? 'en'
    warnIfUnsupportedLang(lang, req.path)

    const db = router.db.getState()
    const item = db.news.find(n => n.id === req.params.id)

    if (!item)
      return res.status(404).json({ error: `News '${req.params.id}' not found` })

    res.json(localizeNode(item, lang))
  })

}
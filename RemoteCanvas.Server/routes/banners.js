const { localizeNode, warnIfUnsupportedLang } = require('./localize')

  // GET /active_banner — returns current active banner
  // PATCH /banners/:id/activate — activates banner, deactivates the rest
module.exports = function registerBannerRoutes(server, router) {

  server.get('/active_banner', (req, res) => {
    const lang   = req.query.lang ?? 'en'
    warnIfUnsupportedLang(lang, req.path)

    const db     = router.db.getState()
    const banner = db.banners.find(b => b.active)

    if (!banner)
      return res.status(404).json({ error: 'No active banner' })

    res.json(localizeNode(banner, lang))
  })

  server.patch('/banners/:id/activate', (req, res) => {
    const { id } = req.params
    const db     = router.db.getState()

    const banner = db.banners.find(b => b.id === id)
    if (!banner)
      return res.status(404).json({ error: `Banner '${id}' not found` })

    router.db.get('banners')
      .each(b => { b.active = false })
      .write()

    router.db.get('banners')
      .find({ id })
      .assign({ active: true })
      .write()

    const updated = router.db.get('banners').find({ id }).value()
    res.json(updated)
  })

}
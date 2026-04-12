const { localizeNode, warnIfUnsupportedLang } = require('./localize')

// GET /gallery_items         — list with lang support
// POST /gallery_items/:id/like   — like an item
// DELETE /gallery_items/:id/like — unlike an item
module.exports = function registerGalleryRoutes(server, router) {

  server.get('/gallery_items', (req, res) => {
    const db = router.db.getState()
    const lang = req.query.lang ?? 'en'
    warnIfUnsupportedLang(lang, req.path)

    res.json(db.gallery_items.map(item => localizeNode(item, lang)))
  })

  server.post('/gallery_items/:id/like', (req, res) => {
    const db = router.db.getState()
    const { id } = req.params
    const { userId } = req.body

    if (!userId)
      return res.status(400).json({ error: 'userId is required' })

    const item = db.gallery_items.find(i => i.id === id)
    if (!item)
      return res.status(404).json({ error: `Item '${id}' not found` })

    const alreadyLiked = db.liked_items.some(
      l => l.itemId === id && l.userId === userId
    )
    if (alreadyLiked)
      return res.status(409).json({ error: 'Already liked', likes: item.likes })

    router.db.get('gallery_items').find({ id }).assign({ likes: item.likes + 1 }).write()
    router.db.get('liked_items').push({ id: `${userId}_${id}`, itemId: id, userId }).write()

    const updated = router.db.get('gallery_items').find({ id }).value()
    res.json({ likes: updated.likes })
  })

  server.delete('/gallery_items/:id/like', (req, res) => {
    const db = router.db.getState()
    const { id } = req.params
    const { userId } = req.query

    if (!userId)
      return res.status(400).json({ error: 'userId is required' })

    const item = db.gallery_items.find(i => i.id === id)
    if (!item)
      return res.status(404).json({ error: `Item '${id}' not found` })

    const likeRecord = db.liked_items.find(
      l => l.itemId === id && l.userId === userId
    )
    if (!likeRecord)
      return res.status(409).json({ error: 'Not liked yet', likes: item.likes })

    router.db.get('gallery_items')
      .find({ id })
      .assign({ likes: Math.max(0, item.likes - 1) })
      .write()
    router.db.get('liked_items').remove({ itemId: id, userId }).write()

    const updated = router.db.get('gallery_items').find({ id }).value()
    res.json({ likes: updated.likes })
  })
}
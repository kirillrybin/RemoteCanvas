const jsonServer = require('json-server')
const fs = require('fs')
const path = require('path')
const registerPagesRoute = require('./routes/pages')
const registerGalleryRoutes = require('./routes/galleryItems')
const registerBannerRoutes = require('./routes/banners')
const registerNewsRoutes = require('./routes/news')

const IS_DEV    = process.env.NODE_ENV !== 'production'
const PORT      = process.env.PORT ?? 3000
const DB_PATH   = path.join(__dirname, 'data/db.json')
const SEED_PATH = path.join(__dirname, 'data/db.seed.json')

const server = jsonServer.create()
const router = jsonServer.router('data/db.json')
const middlewares = jsonServer.defaults()

server.use(middlewares)
server.use(jsonServer.bodyParser)


if (IS_DEV) {
  server.post('/reset', (req, res) => {
    const seed = fs.readFileSync(SEED_PATH, 'utf-8')
    fs.writeFileSync(DB_PATH, seed, 'utf-8')
    router.db.read()
    res.json({ ok: true })
  })
}

registerPagesRoute(server, router)
registerGalleryRoutes(server, router)
registerBannerRoutes(server, router)
registerNewsRoutes(server, router)

server.use(router)

server.listen(PORT, () => {
  console.log(`SDUI Mock Server running at http://localhost:${PORT}`)
  console.log('')
  console.log('Available pages:')
  const db = router.db.getState()
  db.pages.forEach(p => {
    console.log(`  GET http://localhost:${PORT}/pages/${p.id}`)
  })
})
import './FondoBlobs.css'

/** Manchas radiales difuminadas y animadas, fijas detrás de todo el layout. */
export function FondoBlobs() {
  return (
    <div className="fondo-blobs" aria-hidden="true">
      <div className="fondo-blob fondo-blob-mint" />
      <div className="fondo-blob fondo-blob-gold" />
    </div>
  )
}

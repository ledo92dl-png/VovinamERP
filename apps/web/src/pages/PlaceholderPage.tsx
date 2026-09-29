type PlaceholderPageProps = {
  eyebrow: string
  title: string
  description: string
}

export default function PlaceholderPage({
  eyebrow,
  title,
  description,
}: PlaceholderPageProps) {
  return (
    <div className="page">
      <header className="page-header">
        <div>
          <span className="eyebrow">{eyebrow}</span>
          <h1>{title}</h1>
          <p>{description}</p>
        </div>
      </header>

      <section className="notice-card">
        <div>
          <span className="eyebrow">MVP</span>
          <h2>Màn hình đã sẵn sàng để kết nối API</h2>
          <p>
            Chúng ta sẽ hoàn thiện chức năng này ở các gói frontend tiếp theo.
          </p>
        </div>
      </section>
    </div>
  )
}

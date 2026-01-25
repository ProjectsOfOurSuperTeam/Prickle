import { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  HiClock, HiCurrencyDollar, HiCheckCircle, HiAcademicCap, HiSparkles, HiDeviceMobile,
  HiCube, HiShieldCheck, HiBeaker, HiColorSwatch, HiBookOpen, HiDocumentDownload
} from 'react-icons/hi';
import s1 from '../assets/images/decor/s1.png';
import s5 from '../assets/images/decor/s5.png';
import s8 from '../assets/images/decor/s8.png';
import s12 from '../assets/images/decor/s12.png';
import s15 from '../assets/images/decor/s15.png';
import s20 from '../assets/images/decor/s20.png';
import s25 from '../assets/images/decor/s25.png';
import s30 from '../assets/images/decor/s30.png';
import './LandingPage.css';

// Landing Page: Presentation of Prickle capabilities and gallery of examples
function LandingPage() {
  const [openFaq, setOpenFaq] = useState(null);

  const toggleFaq = (index) => {
    setOpenFaq(openFaq === index ? null : index);
  };

  const faqData = [
    {
      question: "Чи потрібна реєстрація для використання Prickle?",
      answer: "Ні, можна переглядати каталог та експериментувати з конструктором без реєстрації. Але для збереження проєктів, експорту PDF та використання AI-генерації потрібен обліковий запис."
    },
    {
      question: "Як працює перевірка сумісності рослин?",
      answer: "Система аналізує потребами рослин у світлі, воді, температурі та темпі росту. Якщо рослини несумісні (наприклад, мох і кактус), ви отримаєте попередження з поясненням конфлікту та рекомендаціями."
    },
    {
      question: "Чи безкоштовно використання Prickle?",
      answer: "Так, базовий функціонал безкоштовний. Ви можете створювати проєкти, перевіряти сумісність та отримувати рецепти ґрунту. AI-генерація зображень може мати обмеження для безкоштовних користувачів."
    },
    {
      question: "Як експортувати проєкт?",
      answer: "Після створення композиції натисніть кнопку \"Експортувати\" та оберіть формат PDF. Ви отримаєте файл зі списком покупок, рецептом ґрунту та покроковою інструкцією зі створення флораріуму."
    },
    {
      question: "Чи можна використовувати Prickle на мобільному?",
      answer: "Так, наш вебзастосунок адаптований для мобільних пристроїв. Ви можете переглядати каталог та створювати проєкти на смартфоні або планшеті."
    },
    {
      question: "Як часто оновлюється каталог рослин?",
      answer: "Каталог постійно поповнюється новими рослинами, формами та декоративними елементами. Адміністратори додають новий контент регулярно на основі запитів користувачів."
    }
  ];

  return (
    <div className="landing-page">
      {/* Hero Section */}
      <section className="hero">
        <img src={s1} alt="Succulent decoration" className="decor decor-1" />
        <img src={s5} alt="Succulent decoration" className="decor decor-2" />
        <img src={s12} alt="Succulent decoration" className="decor decor-3" />
        <div className="hero-content">
          <h1 className="hero-title">
            З <span className="prickle-gradient">Prickle</span> твій флораріум буде <span className="live-gradient">жити</span>!
          </h1>
          <p className="hero-subtitle">
            Ми перетворюємо складну ботаніку на просту інструкцію: моделюй дизайн, отримуй рецепт субстрату та список сумісних рослин за 5 хвилин.
          </p>
          <div className="hero-actions">
            <Link to="/constructor" className="btn btn-primary">
              Створити флораріум
            </Link>
            <Link to="/catalog" className="btn btn-secondary">
              Переглянути каталог
            </Link>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="features">
        <img src={s8} alt="Succulent decoration" className="decor decor-4" />
        <img src={s20} alt="Succulent decoration" className="decor decor-5" />
        <div className="container">
          <h2 className="section-title">Можливості Prickle</h2>
          <div className="features-grid">
            <div className="feature-card">
              <div className="feature-icon">
                <HiCube />
              </div>
              <h3>Інтерактивний конструктор</h3>
              <p>
                Drag-and-Drop інтерфейс для створення композицій. Обирайте форму контейнера,
                додавайте рослини та декор у зручному 2.5D редакторі.
              </p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">
                <HiShieldCheck />
              </div>
              <h3>Перевірка сумісності</h3>
              <p>
                Автоматична перевірка біологічної сумісності рослин. Система попереджає про
                конфлікти та допомагає створити стійку екосистему.
              </p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">
                <HiBeaker />
              </div>
              <h3>Генератор ґрунту</h3>
              <p>
                Автоматичний розрахунок ідеальної формули ґрунту на основі обраних рослин.
                Отримуйте точні пропорції компонентів субстрату.
              </p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">
                <HiColorSwatch />
              </div>
              <h3>AI-візуалізація</h3>
              <p>
                Генерація реалістичного зображення вашого флораріуму за допомогою штучного інтелекту.
                Побачте результат ще до початку роботи.
              </p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">
                <HiBookOpen />
              </div>
              <h3>Каталог компонентів</h3>
              <p>
                Велика база рослин, форм контейнерів та декоративних елементів з детальною
                інформацією про кожен компонент.
              </p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">
                <HiDocumentDownload />
              </div>
              <h3>Експорт проєкту</h3>
              <p>
                Генерація PDF зі списком покупок та покроковою інструкцією зі створення
                вашого флораріуму.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* How it works Section */}
      <section className="how-it-works">
        <img src={s15} alt="Succulent decoration" className="decor decor-6" />
        <div className="container">
          <h2 className="section-title">Як це працює</h2>
          <p className="section-subtitle">
            Створення флораріуму за 6 простих кроків
          </p>
          <div className="steps">
            <div className="step">
              <div className="step-number">1</div>
              <h3>Обери форму контейнера</h3>
              <p>
                Виберіть скляну ємність з каталогу: від мініатюрних до великих форм.
                Кожна форма має свої особливості та рекомендації.
              </p>
            </div>
            <div className="step">
              <div className="step-number">2</div>
              <h3>Додай рослини та декор</h3>
              <p>
                Перетягуй рослини з каталогу на канвас. Додавай декоративні елементи:
                каміння, пісок, фігурки. Все в зручному 2.5D редакторі.
              </p>
            </div>
            <div className="step">
              <div className="step-number">3</div>
              <h3>Перевір сумісність</h3>
              <p>
                Система автоматично перевіряє біологічну сумісність рослин.
                Отримуй попередження про можливі конфлікти та рекомендації.
              </p>
            </div>
            <div className="step">
              <div className="step-number">4</div>
              <h3>Отримай рецепт ґрунту</h3>
              <p>
                Автоматичний розрахунок ідеальної формули субстрату з точними
                пропорціями компонентів для твоєї композиції.
              </p>
            </div>
            <div className="step">
              <div className="step-number">5</div>
              <h3>Побач AI-візуалізацію</h3>
              <p>
                AI генерує реалістичне зображення твого флораріуму. Побач, як він
                виглядатиме в реальному житті з урахуванням освітлення та об'єму.
              </p>
            </div>
            <div className="step">
              <div className="step-number">6</div>
              <h3>Експортуй проєкт</h3>
              <p>
                Отримай готовий PDF зі списком покупок, рецептом ґрунту та
                покроковою інструкцією зі створення твого флораріуму.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Problems Section */}
      <section className="problems">
        <div className="container">
          <h2 className="section-title">Проблеми, які вирішує Prickle</h2>
          <div className="problems-list">
            <div className="problem-item">
              <h3>Біологічна несумісність</h3>
              <p>
                Висаджування рослин з різними потребами у волозі в одну ємність призводить до
                загибелі рослин. Prickle блокує помилкові поєднання ще на етапі дизайну.
              </p>
            </div>
            <div className="problem-item">
              <h3>Помилки з ґрунтом</h3>
              <p>
                Новачки часто використовують універсальну землю, яка не підходить для сукулентів.
                Отримуйте точні інструкції з правильного нашарування ґрунту.
              </p>
            </div>
            <div className="problem-item">
              <h3>Складність візуалізації</h3>
              <p>
                Важко спрогнозувати, як виглядатиме флораріум у реальному житті. AI-генерація
                дозволяє побачити результат заздалегідь.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Benefits Section */}
      <section className="benefits">
        <img src={s25} alt="Succulent decoration" className="decor decor-7" />
        <img src={s30} alt="Succulent decoration" className="decor decor-8" />
        <div className="container">
          <h2 className="section-title">Переваги використання Prickle</h2>
          <div className="benefits-grid">
            <div className="benefit-item">
              <div className="benefit-icon">
                <HiClock />
              </div>
              <h3>Економія часу</h3>
              <p>
                Не потрібно вивчати ботаніку з нуля. Отримуй готові рішення та
                інструкції за 5 хвилин замість годин досліджень.
              </p>
            </div>
            <div className="benefit-item">
              <div className="benefit-icon">
                <HiCurrencyDollar />
              </div>
              <h3>Економія грошей</h3>
              <p>
                Уникай помилок, які призводять до загибелі рослин. Створюй стійкі
                композиції з першого разу без необхідності переробляти.
              </p>
            </div>
            <div className="benefit-item">
              <div className="benefit-icon">
                <HiCheckCircle />
              </div>
              <h3>Гарантія успіху</h3>
              <p>
                Автоматична перевірка сумісності та точні інструкції з ґрунту
                забезпечують високу ймовірність успішного створення флораріуму.
              </p>
            </div>
            <div className="benefit-item">
              <div className="benefit-icon">
                <HiAcademicCap />
              </div>
              <h3>Навчання</h3>
              <p>
                Дізнавайся про рослини, їх потреби та правильний догляд через
                детальну інформацію в каталозі та рекомендації системи.
              </p>
            </div>
            <div className="benefit-item">
              <div className="benefit-icon">
                <HiSparkles />
              </div>
              <h3>Креативність</h3>
              <p>
                Експериментуй з різними комбінаціями без ризику. Побач результат
                заздалегідь завдяки AI-візуалізації.
              </p>
            </div>
            <div className="benefit-item">
              <div className="benefit-icon">
                <HiDeviceMobile />
              </div>
              <h3>Зручність</h3>
              <p>
                Все в одному місці: від дизайну до списку покупок. Експортуй проєкт
                на будь-який пристрій та працюй з ним офлайн.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Gallery Section */}
      <section className="gallery">
        <div className="container">
          <h2 className="section-title">Галерея прикладів</h2>
          <p className="section-subtitle">
            Надихайтесь створеними композиціями
          </p>
          <div className="gallery-grid">
            <div className="gallery-placeholder">
              <p>Галерея буде доступна після створення перших проєктів</p>
            </div>
          </div>
        </div>
      </section>

      {/* FAQ Section */}
      <section className="faq">
        <div className="container">
          <h2 className="section-title">Часті питання</h2>
          <div className="faq-list">
            {faqData.map((item, index) => (
              <div key={index} className={`faq-item ${openFaq === index ? 'open' : ''}`}>
                <button
                  className="faq-question-btn"
                  onClick={() => toggleFaq(index)}
                  aria-expanded={openFaq === index}
                >
                  <h3 className="faq-question">{item.question}</h3>
                  <span className="faq-icon">{openFaq === index ? '−' : '+'}</span>
                </button>
                {openFaq === index && (
                  <div className="faq-answer-wrapper">
                    <p className="faq-answer">{item.answer}</p>
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="cta">
        <div className="container">
          <h2 className="cta-title">Готові створити свій флораріум?</h2>
          <p className="cta-text">
            Почніть проєктування зараз та отримайте готовий план зі створення унікальної композиції
          </p>
          <Link to="/constructor" className="btn btn-primary btn-large">
            Почати створення
          </Link>
        </div>
      </section>
    </div>
  );
}

export default LandingPage;

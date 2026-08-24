import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import en from './locales/en'
import de from './locales/de'
import fr from './locales/fr'
import uk from './locales/uk'
import es from './locales/es'
import pl from './locales/pl'
import it from './locales/it'
import sv from './locales/sv'
import no from './locales/no'

const savedLang = localStorage.getItem('lang') ?? 'en'

i18n
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      de: { translation: de },
      fr: { translation: fr },
      uk: { translation: uk },
      es: { translation: es },
      pl: { translation: pl },
      it: { translation: it },
      sv: { translation: sv },
      no: { translation: no },
    },
    lng: savedLang,
    fallbackLng: 'en',
    interpolation: { escapeValue: false },
  })

export default i18n

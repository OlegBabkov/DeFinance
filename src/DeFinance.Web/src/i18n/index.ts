import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import en from './locales/en'
import de from './locales/de'
import fr from './locales/fr'

const savedLang = localStorage.getItem('lang') ?? 'en'

i18n
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      de: { translation: de },
      fr: { translation: fr },
    },
    lng: savedLang,
    fallbackLng: 'en',
    interpolation: { escapeValue: false },
  })

export default i18n
